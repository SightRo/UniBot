using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Abstraction;
using UniBot.Core.Settings;
using VkNet;
using VkNet.Abstractions;
using VkNet.Infrastructure;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace UniBot.Vkontakte
{
    public class VkStartup : IMessengerStartup
    {
        public void Init(IBot bot, IServiceCollection services, out IMessenger messenger, out SettingsBase settings)
        {
            var vkSettings = new VkSettings();
            bot.Settings.Messengers[Constants.Name].Bind(vkSettings);
            
            var api = new VkApi();
            api.Authorize(new ApiAuthParams
            {
                AccessToken = vkSettings.Token,
            });
            api.RequestsPerSecond = 20;

            services.AddSingleton<IVkApi>(api);
            services.AddSingleton<VkSettings>(vkSettings);
            
            // TODO Need to figure out how to delete webhook server after shutdown.
            // Probably IApplicationLifetime is the answer.
            
            // var serverId = api.Groups.AddCallbackServer(vkSettings.GroupId, 
            //     bot.Settings.Endpoint + Constants.Endpoint, 
            //     "UniBot.Vk", 
            //     vkSettings.Token);
            //
            // api.Groups.SetCallbackSettings(new CallbackServerParams
            // {
            //     GroupId = vkSettings.GroupId,
            //     ServerId = serverId,
            //     CallbackSettings = new CallbackSettings()
            //     {
            //         MessageNew = true
            //     },
            // });
            
            messenger = new VkMessenger(api);
            settings = vkSettings;
        }
    }
}