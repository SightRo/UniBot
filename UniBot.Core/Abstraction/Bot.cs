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
            if (context.Message != null && CommandBase.TryParseCommand(context.Message.Text, out var commnadName))
                GetCommand(commnadName)?.Execute(context);
        }

        public Bot Init(IServiceCollection services)
        {
            var builder = services.AddControllers();
            List<IStartUpManager> managers = new List<IStartUpManager>();
            var assemblies = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .Where(ass => ass.GetCustomAttribute<MessengerImplAttribute>() != null);
            
            foreach (var assembly in assemblies)
            {
                builder.AddApplicationPart(assembly);
                var temp = assembly.GetTypes()
                                   .Where(x => x.GetInterface(nameof(IStartUpManager)) != null && !x.IsInterface && !x.IsAbstract)
                                   .Select(x => (IStartUpManager) Activator.CreateInstance(x)!)
                                   .First();
                
                managers.Add(temp);
            }
            
            foreach (var manager in managers)
            {
                manager.Init(this, services, out var messenger, out var settings);
                if (!settings.IsEnabled)
                    continue;
            
                //Owners.TryAdd(messenger.Name, settings.BotOwnerId);
                //Admins.TryAdd(messenger.Name, settings.BotAdminIds);
            }

            var Commands = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(ass => ass.GetTypes())
                                    .Where(x => string.Compare(typeof(CommandBase).ToString(), x?.BaseType?.ToString() ?? string.Empty) == 0 && !x.IsInterface && !x.IsAbstract)
                                    .Select(x => (CommandBase) Activator.CreateInstance(x)!)
                                    .ToDictionary(x => x.Name);

            return this;
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
                throw new Exception("Messenger is missed.");
        }
    }

    public static class BotExtension
    {
        public static void Init(this IServiceCollection services, IConfiguration config)
        {
            var builder = services.AddControllers();
            var bot = new Bot(config);
            
            services.AddSingleton<IBot>(bot);
            List<IStartUpManager> managers = new List<IStartUpManager>();
            var assemblies = AppDomain.CurrentDomain
                                      .GetAssemblies()
                                      .Where(ass => ass.GetCustomAttribute<MessengerImplAttribute>() != null);
            
            foreach (var assembly in assemblies)
            {
                builder.AddApplicationPart(assembly);
                var temp = assembly.GetTypes()
                                   .Where(x => x.GetInterface(nameof(IStartUpManager)) != null && !x.IsInterface && !x.IsAbstract)
                                   .Select(x => (IStartUpManager) Activator.CreateInstance(x)!)
                                   .First();
                
                managers.Add(temp);
            }
            
            foreach (var manager in managers)
            {
                manager.Init(bot, services, out var messenger, out var settings);
                if (!settings.IsEnabled)
                    continue;
            
                //Owners.TryAdd(messenger.Name, settings.BotOwnerId);
                //Admins.TryAdd(messenger.Name, settings.BotAdminIds);
            }

            var Commands = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(ass => ass.GetTypes())
                                    .Where(x => string.Compare(typeof(CommandBase).ToString(), x?.BaseType?.ToString() ?? string.Empty) == 0 && !x.IsInterface && !x.IsAbstract)
                                    .Select(x => (CommandBase) Activator.CreateInstance(x)!)
                                    .Select(Commands.);
        }
    }
}