using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MihaZupan;
using Telegram.Bot;
using UniBot.Core.Abstraction;
using UniBot.Core.Settings;

namespace UniBot.Telegram
{
    public class TgStartupManager : IStartupManager
    {
        public void Init(IBot bot, IServiceCollection services, out IMessenger messenger, out SettingsBase settings)
        {
            var tgSettings = new TgSettings();
            // TODO Error handling.
            bot.Settings.Messengers[Constants.Name].Bind(tgSettings);

            var proxy = GetProxy(tgSettings);
            var api = proxy != null
                ? new TelegramBotClient(tgSettings.Token, proxy)
                : new TelegramBotClient(tgSettings.Token);

            services.AddSingleton<ITelegramBotClient>(api);
            services.AddSingleton(tgSettings);

            api.SetWebhookAsync(bot.Settings.Endpoint + Constants.Endpoint).GetAwaiter().GetResult();
            
            messenger = new TgMessenger(api, tgSettings);
            settings = tgSettings;
        }

        private IWebProxy? GetProxy(TgSettings settings)
        {
            if (settings.Socks5 == null)
                return null;

            var socks5 = settings.Socks5;
            
            return socks5.Username == null || socks5.Password == null
                ? new HttpToSocks5Proxy(socks5.Hostname, socks5.Port) 
                : new HttpToSocks5Proxy(socks5.Hostname, socks5.Port, socks5.Username, socks5.Password);
        }
    }
}