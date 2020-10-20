using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;

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
        }

        public BotBuilder DetectCommands()
        {
            // TODO Find an elegant solution.
            var commands = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(ass => ass.GetTypes())
                                    .Where(x => 
                                        string.CompareOrdinal(typeof(CommandBase).ToString(), x?.BaseType?.ToString() ?? string.Empty) == 0 
                                        && !x.IsInterface 
                                        && !x.IsAbstract);

            foreach (var command in commands)
                AddCommand(command);

            return this;
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