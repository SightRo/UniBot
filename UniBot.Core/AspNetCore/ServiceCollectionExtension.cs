using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;
using UniBot.Core.Settings;

namespace UniBot.Core.AspNetCore
{
    public static class ServiceCollectionExtension
    {
        public static IBotBuilder AddBot(this IServiceCollection services, IConfiguration config)
        {
            var bot = new Bot(config);

            services.AddSingleton<BotSettings>(bot.Settings);
            services.AddSingleton<IBot>(bot);

            var assemblies = AppDomain.CurrentDomain
                                      .GetAssemblies()
                                      .Where(ass => ass.GetCustomAttribute<MessengerImplAttribute>() != null);

            var builder = services.AddControllers();
            var managers = new List<IStartupManager>();
            
            foreach (var assembly in assemblies)
            {
                // TODO Error handling.
                builder.AddApplicationPart(assembly);
                var startup = assembly.GetTypes()
                                      .Where(x => x.GetInterface(nameof(IStartupManager)) != null && !x.IsAbstract && !x.IsInterface)
                                      .Select(x => (IStartupManager)Activator.CreateInstance(x)!)
                                      .First();
                
                managers.Add(startup);
            }

            foreach (var manager in managers)
            {
                manager.Init(bot, services, out var messenger, out var settings);
                if (!settings.IsEnabled)
                    continue;

                bot.RegisterOwner(messenger.Name, settings.BotOwnerId);
                bot.RegisterAdmins(messenger.Name, settings.BotAdminIds);
            }
            
            return new BotBuilder(bot);
        }
    }
}