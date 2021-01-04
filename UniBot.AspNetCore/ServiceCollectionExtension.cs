using System;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core;
using UniBot.Core.Abstraction;

namespace UniBot.AspNetCore
{
    public static class ServiceCollectionExtension
    {
        public static void AddUniBot(this IServiceCollection services, Func<BotBuilder, IBot> builder)
        {
            var bot = builder.Invoke(new BotBuilder());

            services.AddSingleton(bot);
            services.AddSingleton(bot.BotOptions);
        }
    }
}