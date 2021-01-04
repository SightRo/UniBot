using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MihaZupan;
using Telegram.Bot;
using UniBot.Core;
using UniBot.Core.Abstraction;

namespace UniBot.AspNetCore.Telegram
{
    public class TgStartup : IMessengerStartup
    {
        public void Init(IBot bot, out IMessenger messenger)
        {
            var tgOptions = bot.BotOptions.MessengersOptions[TgConstants.Name].Get<TgOptions>();

            var proxy = GetProxy(tgOptions);
            var api = proxy != null
                ? new TelegramBotClient(tgOptions.Token, proxy)
                : new TelegramBotClient(tgOptions.Token);

            try
            {
                api.SetWebhookAsync(bot.BotOptions.Endpoint + TgConstants.Endpoint).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't set telegram webhook", e);
            }

            messenger = new TgMessenger(api, tgOptions);
        }

        private IWebProxy? GetProxy(TgOptions options)
        {
            if (options.Socks5 == null)
                return null;

            var socks5 = options.Socks5;

            return socks5.Username == null || socks5.Password == null
                ? new HttpToSocks5Proxy(socks5.Hostname, socks5.Port)
                : new HttpToSocks5Proxy(socks5.Hostname, socks5.Port, socks5.Username, socks5.Password);
        }
    }
}