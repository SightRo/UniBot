using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UniBot.Core;
using UniBot.Core.Abstraction;
using UniBot.Core.Annotations;
using UniBot.Core.Models;
using VkNet.Utils;
using VkMessage = VkNet.Model.Message;

namespace UniBot.AspNetCore.Vkontakte
{
    [UpdateReceiver(VkConstants.Name)]
    [Route(VkConstants.Endpoint)]
    public class VkReceiver : ControllerBase
    {
        private readonly IBot _bot;
        private readonly VkMessenger _messenger;

        public VkReceiver(IBot bot)
        {
            _bot = bot;
            _messenger = bot.ResolveMessenger(VkConstants.Name) as VkMessenger;
        }

        // TODO Check out GroupUpdate class.
        [HttpPost]
        public async Task<IActionResult> UpdateReceiver([FromBody] Update update)
        { 
            switch (update.Type)
            {
                case "confirmation":
                    return Ok(_messenger.ConfirmationCode);
                default:
                    var context = await CreateUpdateContext(update).ConfigureAwait(false);
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
                    var chat = await _messenger.GetChat(originalMessage.PeerId!.Value).ConfigureAwait(false);
                    var sender = await _messenger.GetUser(originalMessage.FromId!.Value).ConfigureAwait(false);
                    return new UpdateContext(_messenger, chat, sender, message);
                default:
                    return null;
            }
            
        }

    }
}