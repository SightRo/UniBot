using System;
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
            var tgOptions = bot.BotOptions.MessengerOptions[TgConstants.Name].Get<TgOptions>();
            services.Configure<TgOptions>(bot.BotOptions.MessengerOptions[TgConstants.Name]);

            var proxy = GetProxy(tgOptions);
            var api = proxy != null
                ? new TelegramBotClient(tgOptions.Token, proxy)
                : new TelegramBotClient(tgOptions.Token);

            try
            {
                api.SetWebhookAsync(bot.BotOptions.Endpoint + TgConstants.Endpoint).GetAwaiter().GetResult();
            }
            catch(Exception e)
            {
                throw new Exception("Couldn't set telegram webhook", e);
            }

            messenger = new TgMessenger(api, tgOptions);
            services.AddSingleton(messenger);
        }

        private IWebProxy? GetProxy(TgOptions options)
        {
            if (options.Socks5 is null)
                return null;

            var socks5 = options.Socks5;

            return socks5.Username is null || socks5.Password is null
                ? new HttpToSocks5Proxy(socks5.Hostname, socks5.Port)
                : new HttpToSocks5Proxy(socks5.Hostname, socks5.Port, socks5.Username, socks5.Password);
        }
    }
}