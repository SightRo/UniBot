using System;
using System.Linq;
using System.Reflection;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;
using UniBot.Core.Annotations;
using UniBot.Core.Options;

namespace UniBot.Core
{
    public class BotBuilder
    {
        private readonly IBot _bot;

        public BotBuilder(BotOptions options)
        {
            _bot = new Bot(options);
        }

        public BotBuilder DetectCommands()
        {
            var commands = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(ass => ass.GetTypes())
                .Where(x => x.IsSubclassOf(typeof(CommandBase)));

            foreach (var command in commands)
                AddCommand(command);

            return this;
        }

        public BotBuilder DetectCommandsFromAssembly(Assembly assembly)
        {
            var commands = assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(CommandBase)));

            foreach (var command in commands)
                AddCommand(command);

            return this;
        }

        public BotBuilder AddCommand(CommandBase command)
        {
            _bot.AddCommand(command);
            return this;
        }

        public IBot Build()
        {
            _bot.InitializeMessengers();
            return _bot;
        }

        public BotBuilder AddCommand<TCommand>()
            where TCommand : CommandBase
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
            _bot.AddMessengerImplementation(assembly);
            return this;
        }

        private void AddCommand(Type command)
        {
            var instance = (CommandBase?) Activator.CreateInstance(command);
            if (instance == null)
                throw new Exception("Couldn't create a command instance");

            _bot.AddCommand(instance);
        }
    }
}