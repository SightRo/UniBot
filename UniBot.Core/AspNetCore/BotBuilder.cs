using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;
using UniBot.Core.Annotations;

namespace UniBot.Core.AspNetCore
{
    public class BotBuilder
    {
        private readonly Bot _bot;
        private readonly IServiceCollection _services;
        
        public BotBuilder(Bot bot, IServiceCollection services)
        {
            _bot = bot;
            _services = services;
            services.AddHttpClient("BotHttpClient");
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

        public BotBuilder AddCommand(CommandBase command)
        {
            _bot.AddCommand(command);
            return this;
        }

        public void Build()
            => _bot.InitializeMessengers(_services);

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
            
            var builder = _services.AddControllers();
            builder.AddApplicationPart(assembly);

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