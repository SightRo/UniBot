using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Actions;
using UniBot.Core.Settings;

namespace UniBot.Core.Abstraction
{
    public class Bot : IBot
    {
        private readonly Dictionary<string, long> _owners = new Dictionary<string, long>();
        private readonly Dictionary<string, long[]> _admins = new Dictionary<string, long[]>();
        private readonly Dictionary<string, CommandBase> _commands = new Dictionary<string, CommandBase>();
        private readonly Dictionary<string, IMessenger> _messengers = new Dictionary<string, IMessenger>();

        public Bot(IConfiguration configuration)
        {
            configuration.GetSection("BotSettings").Bind(Settings);
        }

        public BotSettings Settings { get; } = new BotSettings();
        public IReadOnlyDictionary<string, long> Owners => _owners;
        public IReadOnlyDictionary<string, long[]> Admins => _admins;
        public IReadOnlyDictionary<string, CommandBase> Commands => _commands;
        public IReadOnlyDictionary<string, IMessenger> Messengers => _messengers;

        public void ProcessUpdate(UpdateContext context)
        {
            if (context.Message != null && CommandBase.TryParseCommand(context.Message.Text, out var commandName))
                GetCommand(commandName)?.Execute(context);
        }

        public IAction? GetCommand(string commandName)
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

        internal void RegisterAdmins(string messenger, long[] ids)
            => AddToDictionary(_admins, messenger, ids);

        internal void RegisterMessenger(string messengerName, IMessenger messenger)
            => AddToDictionary(_messengers, messengerName, messenger);

        internal void RegisterCommand(CommandBase command)
            => AddToDictionary(_commands, command.Name, command);

        internal void RegisterOwner(string messenger, long id)
            => AddToDictionary(_owners, messenger, id);
        
        private void AddToDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
            where TKey : notnull
            where TValue : notnull
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
        }
    }
}