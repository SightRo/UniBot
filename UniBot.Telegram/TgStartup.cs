using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MihaZupan;
using Telegram.Bot;
using UniBot.Core.Abstraction;

namespace UniBot.Telegram
{
    public class TgStartup : IMessengerStartup
    {
        public void Init(Bot bot, IServiceCollection services, out IMessenger messenger)
        {
            var tgSettings = bot.BotOptions.MessengerOptions[TgConstants.Name].Get<TgSettings>();
            services.Configure<TgSettings>(bot.BotOptions.MessengerOptions[TgConstants.Name]);

            var proxy = GetProxy(tgSettings);
            var api = proxy != null
                ? new TelegramBotClient(tgSettings.Token, proxy)
                : new TelegramBotClient(tgSettings.Token);

            api.SetWebhookAsync(bot.BotOptions.Endpoint + TgConstants.Endpoint).GetAwaiter().GetResult();
            
            messenger = new TgMessenger(api, tgSettings);
            services.AddSingleton(messenger);
        }

        private IWebProxy? GetProxy(TgSettings settings)
        {
            if (settings.Socks5 is null)
                return null;

            var socks5 = settings.Socks5;
            
            return socks5.Username is null || socks5.Password is null
                ? new HttpToSocks5Proxy(socks5.Hostname, socks5.Port) 
                : new HttpToSocks5Proxy(socks5.Hostname, socks5.Port, socks5.Username, socks5.Password);
        }
    }
}