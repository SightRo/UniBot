using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Abstraction;
using UniBot.Core.Settings;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace UniBot.Vkontakte
{
    public class VkStartup : IMessengerStartup
    {
        private const string ServerName = "UniBot.Vk";

        public void Init(Bot bot, IServiceCollection services, out IMessenger messenger)
        {
            var vkOptions = bot.BotOptions.MessengerOptions[VkConstants.Name].Get<VkOptions>();
            services.Configure<VkOptions>(bot.BotOptions.MessengerOptions[VkConstants.Name]);

            var api = new VkApi();
            api.Authorize(new ApiAuthParams
            {
                AccessToken = vkOptions.Token,
            });
            api.RequestsPerSecond = 20;
            
            DeleteCallbackServer(api, vkOptions);
            AddCallbackServer(api, bot.BotOptions, vkOptions);

            messenger = new VkMessenger(api);
            services.AddSingleton(messenger);
        }

        private void DeleteCallbackServer(VkApi api, VkOptions options)
        {
            var server = api.Groups.GetCallbackServers(options.GroupId)
                .FirstOrDefault(s => string.CompareOrdinal(s.Title, ServerName) == 0);

            if (server != null)
                api.Groups.DeleteCallbackServer(options.GroupId, (ulong)server.Id);
        }

        private void AddCallbackServer(VkApi api, BotOptions botOptions, VkOptions options)
        {
            var serverId = api.Groups.AddCallbackServer(options.GroupId, 
                botOptions.Endpoint + VkConstants.Endpoint, 
                "UniBot.Vk", 
                null);
            
            api.Groups.SetCallbackSettings(new CallbackServerParams
            {
                GroupId = options.GroupId,
                ServerId = serverId,
                CallbackSettings = new CallbackSettings
                {
                    MessageNew = true,
                }
            });
        } 
    }
}