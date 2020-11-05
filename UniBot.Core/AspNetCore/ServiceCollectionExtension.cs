using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Abstraction;
using UniBot.Core.Options;

namespace UniBot.Core.AspNetCore
{
    public static class ServiceCollectionExtension
    {
        public static BotBuilder AddBot(this IServiceCollection services, IConfiguration config)
        {
            var botSettings = config.GetSection("BotSettings").Get<BotOptions>();
            var bot = new Bot(botSettings);

            services.Configure<BotOptions>(config.GetSection("BotSettings"));
            services.AddSingleton(bot);

            return new BotBuilder(bot, services);
        }
    }
}