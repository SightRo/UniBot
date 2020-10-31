using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UniBot.Core.Abstraction;
using UniBot.Core.Annotations;
using TgMessage = Telegram.Bot.Types.Message;
using TgUser = Telegram.Bot.Types.User;
using TgChat = Telegram.Bot.Types.Chat;

namespace UniBot.Telegram
{
    [UpdateReceiver(TgConstants.Name)]
    [Route(TgConstants.Endpoint)]
    public class TgReceiver : ControllerBase
    {
        private readonly Bot _bot;

        public TgReceiver(Bot bot)
        {
            _bot = bot;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReceiver([FromBody] Update update)
        {
            var context = await CreateContext(update);
            // Dangerous thing
            // Can skip message if context couldn't be created
            if (context != null)
                _bot.ProcessUpdate(context);
            
            return Ok();
        }

        private async Task<UpdateContext?> CreateContext(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = TgConverter.ToMessage(update.Message);
                    var user = TgConverter.ToUser(update.Message.From);
                    // The only way to create chat with ownerId.
                    // GetChat can pass ownerId to Converter.ToChat().
                    var messenger = _bot.ResolveMessenger(message.MessengerSource);
                    var chat = await messenger.GetChat(update.Message.Chat.Id);

                    return new UpdateContext(messenger, chat, user, message);
                }
                default:
                    return null;
            }
        }
    }
}