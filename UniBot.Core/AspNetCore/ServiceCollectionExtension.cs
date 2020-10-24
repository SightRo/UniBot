using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Abstraction;
using UniBot.Core.AspNetCore.BackgroundServices;
using UniBot.Core.Settings;

namespace UniBot.Core.AspNetCore
{
    public static class ServiceCollectionExtension
    {
        public static BotBuilder AddBot(this IServiceCollection services, IConfiguration config)
        {
            var botSettings = config.GetSection("BotSettings").Get<BotOptions>();
            var channel = Channel.CreateUnbounded<JobItem>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleWriter = true,
                SingleReader = false
            });
            var bot = new Bot(botSettings, channel);

            services.Configure<BotOptions>(config.GetSection("BotSettings"));
            services.AddSingleton(bot);
            services.AddSingleton(channel);
            services.AddHostedService<ActionExecutorBackgroundService>();

            return new BotBuilder(bot, services);
        }
    }
}