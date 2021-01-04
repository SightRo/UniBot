using System;
using System.Collections.Generic;
using System.Reflection;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;
using UniBot.Core.Annotations;
using UniBot.Core.Models;
using UniBot.Core.Options;
using UniBot.Core.Utils;

namespace UniBot.Core
{
    public class Bot : IBot
    {
        private readonly Dictionary<string, Command> _commands = new();
        private readonly Dictionary<string, IMessenger> _messengers = new();
        private readonly Dictionary<string, IMessengerStartup> _messengerStartups = new();
        private readonly JobQueue _jobQueue;

        public Bot(BotOptions botOptions)
        {
            BotOptions = botOptions;
            _jobQueue = new JobQueue(botOptions.ExecutingOptions.ThreadsCount);
        }

        public BotOptions BotOptions { get; }
        public IReadOnlyDictionary<string, Command> Commands => _commands;
        public IReadOnlyDictionary<string, IMessenger> Messengers => _messengers;

        public void ProcessUpdate(UpdateContext context)
        {
            if (context.Message != null && Command.TryParseCommand(context.Message.Text, out var commandName))
            {
                var command = GetCommand(commandName);
                if (command == null)
                    return;

                _jobQueue.Enqueue(new JobItem(command, context));
            }
        }

        public Command? GetCommand(string commandName)
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

        public void InitializeMessengers()
        {
            foreach (var item in _messengerStartups)
            {
                item.Value.Init(this, out var messenger);
                _messengers.Add(item.Key, messenger);
            }
        }

        public void AddMessengerImplementation(Assembly assembly)
        {
            var messengerName = Reflector.FindAssemblyAttribute<MessengerImplAttribute>(assembly).Name;
            var startupType = Reflector.FindInterfaceImplementation(assembly, nameof(IMessengerStartup));


            // Check presents of necessary attributes/implementations
            // Otherwise throw an exception
            Reflector.FindInterfaceImplementation(assembly, nameof(IMessenger));
            Reflector.FindSubType(assembly, typeof(MessengerOptions));
            Reflector.FindTypeWithAttribute<UpdateReceiverAttribute>(assembly);

            var startup = (IMessengerStartup) Reflector.GetInstance(startupType);
            _messengerStartups.Add(messengerName, startup);
        }

        public void AddCommand<TCommand>(TCommand command)
            where TCommand : Command
        {
            _commands.Add(command.Name, command);
        }
    }
}