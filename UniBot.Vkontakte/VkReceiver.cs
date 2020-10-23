using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UniBot.Core.Abstraction;
using VkNet.Utils;
using VkMessage = VkNet.Model.Message;

namespace UniBot.Vkontakte
{
    [UpdateReceiver(VkConstants.Name)]
    [Route(VkConstants.Endpoint)]
    public class VkReceiver : ControllerBase
    {
        private string Name => VkConstants.Name;

        private readonly Bot _bot;
        private readonly VkMessenger _messenger;
        private readonly VkOptions _options;

        public VkReceiver(Bot bot, IOptions<VkOptions> options)
        {
            _bot = bot;
            _options = options.Value;
            _messenger = bot.ResolveMessenger(Name) as VkMessenger;

        }

        // TODO Check out GroupUpdate class.
        [HttpPost]
        public async Task<IActionResult> UpdateReceiver([FromBody] Update update)
        { 
            switch (update.Type)
            {
                case "confirmation":
                    return Ok(_options.Confirmation);
                default:
                    var context = await CreateUpdateContext(update);
                    // Dangerous thing
                    // Can skip message if context couldn't be created
                    if (context != null)
                        _bot.ProcessUpdate(context);
                    break;
            }
            
            return Ok("Ok");
        }

        private async Task<UpdateContext?> CreateUpdateContext(Update update)
        {
            switch (update.Type)
            {
                case "message_new":
                    var originalMessage = VkMessage.FromJson(new VkResponse(update.Object));
                    var message = VkConverter.ToInMessage(originalMessage);
                    var chat = await _messenger.GetChat(originalMessage.PeerId!.Value);
                    var sender = await _messenger.GetUser(originalMessage.FromId!.Value);
                    return new UpdateContext(_messenger, chat, sender, message);
                default:
                    return null;
            }
            
        }

    }
}