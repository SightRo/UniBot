using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MihaZupan;
using Telegram.Bot;
using UniBot.Core.Abstraction;
using UniBot.Core.Settings;

namespace UniBot.Telegram
{
    public class TgStartUpManager : IStartUpManager
    {
        public void Init(IBot bot, IServiceCollection services, out IMessenger messenger, out SettingsBase settings)
        {
            var config = bot.Configuration;
            var tgSettings = new TgSettings();
            
            config.GetSection(Constants.Name).Bind(tgSettings);
            services.AddSingleton(tgSettings);

            var proxy = GetProxy(tgSettings);
            var api = proxy != null
                ? new TelegramBotClient(tgSettings.Token, proxy)
                : new TelegramBotClient(tgSettings.Token);

            services.AddSingleton<ITelegramBotClient>(api);

            api.SetWebhookAsync("https://justanotherapptotest1001.herokuapp.com/" + Constants.Endpoint).GetAwaiter().GetResult();
            
            messenger = new TgMessenger(api, tgSettings);
            settings = tgSettings;
        }

        private IWebProxy? GetProxy(TgSettings settings)
        {
            if (settings.Socks5 == null)
                return null;

            var socks5 = settings.Socks5;
            
            return socks5.Username == null 
                ? new HttpToSocks5Proxy(socks5.Hostname, socks5.Port) 
                : new HttpToSocks5Proxy(socks5.Hostname, socks5.Port, socks5.Username, socks5.Password);
        }
    }
}