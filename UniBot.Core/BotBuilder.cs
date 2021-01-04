using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;
using UniBot.Core.Annotations;
using UniBot.Core.Options;
using UniBot.Core.Utils;

namespace UniBot.Core
{
    public class BotBuilder
    {
        private readonly List<Command> _commands = new();
        private readonly List<Assembly> _messengerImpls = new();
        private BotOptions? _botOptions;


        public BotBuilder DetectCommands()
        {
            var commands = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(ass => ass.GetTypes())
                .Where(x => x.IsSubclassOf(typeof(Command)));

            foreach (var command in commands)
                AddCommand(command);

            return this;
        }

        public BotBuilder DetectCommandsFromAssembly(Assembly assembly)
        {
            var commands = assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(Command)));

            foreach (var command in commands)
                AddCommand(command);

            return this;
        }

        public BotBuilder AddCommand(Command command)
        {
            _commands.Add(command);
            return this;
        }

        public IBot Build<TBot>()
            where TBot : class, IBot
        {
            var bot = Reflector.GetInstance<TBot>(_botOptions) as IBot;
            return InitializeBot(bot);
        }

        public IBot Build()
        {
            var bot = new Bot(_botOptions);
            return InitializeBot(bot);
        }

        public BotBuilder AddCommand<TCommand>()
            where TCommand : Command
        {
            AddCommand(typeof(TCommand));
            return this;
        }

        public BotBuilder DetectMessengerImplementations()
        {
            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(ass => ass.GetCustomAttribute<MessengerImplAttribute>() != null);

            foreach (var assembly in assemblies)
                AddMessengerImplementation(assembly);

            return this;
        }

        public BotBuilder AddMessengerImplementation(Assembly assembly)
        {
            _messengerImpls.Add(assembly);
            return this;
        }

        public BotBuilder WithOptions(BotOptions options)
        {
            _botOptions = options;
            return this;
        }

        private void AddCommand(Type command)
        {
            var instance = (Command?) Activator.CreateInstance(command);
            if (instance == null)
                throw new Exception("Couldn't create a command instance");

            _commands.Add(instance);
        }

        private IBot InitializeBot(IBot bot)
        {
            foreach (var command in _commands)
                bot.AddCommand(command);
            foreach (var messengerImpl in _messengerImpls)
                bot.AddMessengerImplementation(messengerImpl);

            bot.InitializeMessengers();
            return bot;
        }
    }
}