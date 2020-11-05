using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Actions;
using UniBot.Core.Annotations;
using UniBot.Core.Options;
using UniBot.Core.Utils;

namespace UniBot.Core.Abstraction
{
    public class Bot
    {
        private readonly Dictionary<string, CommandBase> _commands = new Dictionary<string, CommandBase>();
        private readonly Dictionary<string, IMessenger> _messengers = new Dictionary<string, IMessenger>();
        private readonly List<IMessengerStartup> _messengerStartups = new List<IMessengerStartup>();
        private readonly JobQueue _jobQueue;
        
        public Bot(BotOptions botOptions)
        {
            BotOptions = botOptions;
            _jobQueue = new JobQueue(botOptions.ExecutingOptions.ThreadsCount);
        }

        public BotOptions BotOptions { get; }
        public IReadOnlyDictionary<string, CommandBase> Commands => _commands;
        public IReadOnlyDictionary<string, IMessenger> Messengers => _messengers;

        public void ProcessUpdate(UpdateContext context)
        {
            if (context.Message != null && CommandBase.TryParseCommand(context.Message.Text, out var commandName))
            {
                var command = GetCommand(commandName);
                if (command is null)
                    return;
                
                _jobQueue.Enqueue(new JobItem(command, context));
            }
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

            var startupType = GetInterfaceImplementation(assembly, nameof(IMessengerStartup));
            GetInterfaceImplementation(assembly, nameof(IMessenger));
            GetAttributeUsage<UpdateReceiverAttribute>(assembly);

            var startup = Reflector.GetInstance<IMessengerStartup>(startupType);
            AddToCollection(_messengerStartups, startup);
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

        private Type GetInterfaceImplementation(Assembly assembly, string interfaceName)
        {
            var implementations = Reflector.FindInterfaceImplementations(assembly, interfaceName);

            if (implementations.Count == 0)
                throw new Exception("Not found necessary interface");
            if (implementations.Count > 1)
                throw new Exception("Found more than one interface implementation");

            return implementations[0];
        }

        private Type GetAttributeUsage<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            var usages = Reflector.FindAttributeUsage<TAttribute>(assembly);

            if (usages.Count == 0)
                throw new Exception("Not found necessary type with attribute");
            if (usages.Count > 1)
                throw new Exception("Found more than one type with attribute");

            return usages[0];
        }
    }
}