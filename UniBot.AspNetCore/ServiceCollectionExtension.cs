using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core;
using UniBot.Core.Abstraction;
using UniBot.Core.Options;

namespace UniBot.AspNetCore
{
    public static class ServiceCollectionExtension
    {
        public static void AddUniBot(this IServiceCollection services, Func<BotBuilder, IBot> builder)
        {
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var options = config.GetSection("BotSettings").Get<BotOptions>();
            var bot = builder?.Invoke(new BotBuilder(options));

            services.AddSingleton(bot);
            services.AddSingleton(options);
        }
    }
}