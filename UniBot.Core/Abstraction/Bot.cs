using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Actions;

namespace UniBot.Core.Abstraction
{
    public class Bot : IBot
    {
        public Bot(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public IConfiguration Configuration { get; }
        
        public ConcurrentDictionary<string, long> Owners { get; private set; }
        public ConcurrentDictionary<string, long[]> Admins { get; private set; }
        public ConcurrentDictionary<string, CommandBase> Commands { get; private set; }
        public ConcurrentDictionary<string, IMessenger> Messengers { get; private set; }

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
}