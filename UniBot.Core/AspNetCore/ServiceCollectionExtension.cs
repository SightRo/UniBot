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
            var botSettings = new BotSettings();
            // TODO Change way of getting bot settings;
            config.GetSection("BotSettings").Bind(botSettings);
            var bot = new Bot(botSettings);

            services.AddSingleton<BotSettings>(bot.Settings);
            services.AddSingleton<IBot>(bot);

            return new BotBuilder(bot, services);
        }
    }
}