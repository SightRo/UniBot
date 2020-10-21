using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Actions;
using UniBot.Core.Settings;
using UniBot.Core.Utils;

namespace UniBot.Core.Abstraction
{
    public class Bot
    {
        private readonly Dictionary<string, CommandBase> _commands = new Dictionary<string, CommandBase>();
        private readonly Dictionary<string, IMessenger> _messengers = new Dictionary<string, IMessenger>();
        private readonly List<IMessengerStartup> _messengerStartups = new List<IMessengerStartup>();

        public Bot(BotOptions botOptions)
        {
            BotOptions = botOptions;
        }

        public BotOptions BotOptions { get; }
        public IReadOnlyDictionary<string, CommandBase> Commands => _commands;
        public IReadOnlyDictionary<string, IMessenger> Messengers => _messengers;

        public void ProcessUpdate(UpdateContext context)
        {
            if (context.Message != null && CommandBase.TryParseCommand(context.Message.Text, out var commandName))
                GetCommand(commandName)?.Execute(context);
        }

        public CommandBase? GetCommand(string commandName)
        {
            if (Commands.TryGetValue(commandName, out var command))
                return command;
            return null;
        }

        public IMessenger ResolveMessenger(string name)
        {
            if (Messengers.TryGetValue(name, out var messenger))
                return messenger;
            else
                throw new Exception($"IMessenger {name} is missed.");
        }

        public void InitializeMessengers(IServiceCollection services)
        {
            foreach (var startup in _messengerStartups)
            {
                startup.Init(this, services, out var messenger);
                AddToCollection(_messengers, new KeyValuePair<string, IMessenger>(messenger.Name, messenger));
            }
        }

        public void AddMessengerImplementation(Assembly assembly)
        {
            if (!Reflector.CheckAssemblyAttribute<MessengerImplAttribute>(assembly))
                throw new Exception("Assembly doesn't implement any messenger");

            CheckInterfaceImplementation<IMessengerStartup>(assembly, out var startup);
            CheckInterfaceImplementation<IMessenger>(assembly, out _);
            CheckAttributeUsage<UpdateReceiverAttribute>(assembly);

            if (startup != null)
                AddToCollection(_messengerStartups, startup);
            else
                throw new Exception("Couldn't create instance of IMessengerStartup");
        }

        public void AddCommand<TCommand>(TCommand command)
            where TCommand : CommandBase
            => AddToCollection(_commands, new KeyValuePair<string, CommandBase>(command.Name, command));

        private void AddToCollection<TValue>(ICollection<TValue> collection, TValue value)
            where TValue : notnull
        {
            if (collection.Contains(value))
                throw new Exception("This object has been added");

            collection.Add(value);
        }

        private void CheckInterfaceImplementation<TInterface>(Assembly assembly, out TInterface? instance)
            where TInterface : class
        {
            var implementations = Reflector.FindInterfaceImplementations<TInterface>(assembly);

            if (implementations.Count == 0)
                throw new Exception("Not found necessary interface");
            else if (implementations.Count > 1)
                throw new Exception("Found more than one interface implementation");

            instance = Activator.CreateInstance(implementations[0]) as TInterface;
        }

        private void CheckAttributeUsage<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            var usages = Reflector.FindAttributeUsage<TAttribute>(assembly);

            if (usages.Count == 0)
                throw new Exception("Not found necessary type with attribute");
            else if (usages.Count > 1)
                throw new Exception("Found more than one type with attribute");
        }
    }
}