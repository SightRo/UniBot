using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;

namespace UniBot.Core.AspNetCore
{
    public class BotBuilder : IBotBuilder
    {
        private readonly Bot _bot;
        private readonly IServiceCollection _services;
        
        public BotBuilder(Bot bot, IServiceCollection services)
        {
            _bot = bot;
            _services = services;
        }

        public IBotBuilder DetectCommands()
        {
            // TODO Find an elegant solution.
            var commands = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(ass => ass.GetTypes())
                                    .Where(x => string.CompareOrdinal(typeof(CommandBase).ToString(), x?.BaseType?.ToString() ?? string.Empty) == 0 
                                                && !x.IsInterface 
                                                && !x.IsAbstract);

            foreach (var command in commands)
                AddCommand(command);

            return this;
        }

        public IBotBuilder AddCommand<TCommand>()
            where TCommand : CommandBase
        {
            AddCommand(typeof(TCommand));
            return this;
        }

        public IBotBuilder DetectMessengerImplementations()
        {
            var assemblies = AppDomain.CurrentDomain
                                      .GetAssemblies()
                                      .Where(ass => ass.GetCustomAttribute<MessengerImplAttribute>() != null)
                                      .ToArray();

            foreach (var assembly in assemblies)
                AddMessengerStartup(assembly);

            return this;
        }

        public IBotBuilder AddMessenger(Assembly assembly)
        {
            // TODO Add some checks here
            AddMessengerStartup(assembly);
            return this;
        }

        // Carefully use;
        private void AddCommand(Type command)
        {
            var instance = (CommandBase) Activator.CreateInstance(command);
            _bot.RegisterCommand(instance);
        }

        private void AddMessengerStartup(Assembly assembly)
        {
            var builder = _services.AddControllers();
            
            builder.AddApplicationPart(assembly);
            var startup = assembly.GetTypes()
                                  .Where(x => x.GetInterface(nameof(IMessengerStartup)) != null && !x.IsAbstract && !x.IsInterface)
                                  .Select(x => (IMessengerStartup)Activator.CreateInstance(x)!)
                                  .First();
            
            ActivateMessenger(startup);
        }

        private void ActivateMessenger(IMessengerStartup messengerStartup)
        {
            messengerStartup.Init(_bot, _services, out var messenger, out var settings);
            if (!settings.IsEnabled)
                return;

            _bot.RegisterMessenger(messenger);
            _bot.RegisterOwner(messenger.Name, settings.BotOwnerId);
            _bot.RegisterAdmins(messenger.Name, settings.BotAdminIds);
        }
    }
}