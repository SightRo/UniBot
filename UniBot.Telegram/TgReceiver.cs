using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UniBot.Core.Abstraction;
using UniBot.Core.Models;
using UniBot.Core.Models.Attachments;
using Chat = UniBot.Core.Models.Chat;
using User = UniBot.Core.Models.User;
using TgMessage = Telegram.Bot.Types.Message;
using TgUser = Telegram.Bot.Types.User;
using TgChat = Telegram.Bot.Types.Chat;

namespace UniBot.Telegram
{
    [Route(Constants.Endpoint)]
    public class TgReceiver : Controller
    {
        private string Name => Constants.Name;
        
        private IBot _bot;
        private ITelegramBotClient _api; 
        private TgSettings _settings;

        public TgReceiver(IBot bot, ITelegramBotClient api, TgSettings settings)
        {
            _bot = bot;
            _api = api;
            _settings = settings;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReceiver([FromBody] Update update)
        {
            var context = await Convert(update);
            _bot.ProcessUpdate(context);
            return Ok();
        }

        private async Task<UpdateContext> Convert(Update update)
        {
            var context = update.Type switch
            {
                UpdateType.Message => await ConvertFromMessage(update),
                _ => null
            };
            
            return context!;
        }

        private async Task<UpdateContext> ConvertFromMessage(Update update)
        {
            var message = await ConvertMessage(update.Message);
            var messenger = _bot.ResolveMessenger(Name);

            return new UpdateContext(messenger, message.Chat, message.Sender, message);
        }

        private async Task<InMessage?> ConvertMessage(TgMessage? message)
        {
            if (message == null) return null;

            //TODO Forwaded messages is missing
            return new InMessage
            {
                Id = message.MessageId,
                Date = message.Date,
                Sender = ConvertUser(message.From),
                Chat = await ConvertChat(message.Chat, message),
                Text = message.Text ?? message.Caption,
                Reply = await ConvertMessage(message.ReplyToMessage),
                Attachments = ExtractAttachments(message),
                MessengerSource = Name
            };
        }

        private async Task<Chat> ConvertChat(TgChat chat, TgMessage message)
        {
            var isUserConversation = chat.Type == ChatType.Private;
            long owner = isUserConversation 
                ? message.From.Id
                : (await _api.GetChatAdministratorsAsync(chat.Id)).First(u => u.Status == ChatMemberStatus.Creator).User.Id;
            
            return new Chat
            {
                Id = chat.Id,
                Name = chat.Title ?? chat.Username,
                Owner = owner,
                IsUserConversation = isUserConversation,
                MessengerSource = Name
            };
        }
        
        private User ConvertUser(TgUser user)
        {
            return new User
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsHuman = !user.IsBot,
                MessengerSource = Name
            };
        }
        
        private InAttachment[] ExtractAttachments(TgMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Photo:
                    return new[]
                    {
                        new InAttachment(message.Photo.Last().FileId, AttachmentType.Photo, message.Photo.Last(),
                            Name, null)
                    };
                case MessageType.Audio:
                    return new[]
                    {
                        new InAttachment(message.Audio.FileId, AttachmentType.Audio, message.Audio, Name, null)
                    };
                case MessageType.Video:
                    return new[]
                    {
                        new InAttachment(message.Video.FileId, AttachmentType.Video, message.Video, Name, null)
                    };
                case MessageType.Voice:
                    return new[]
                    {
                        new InAttachment(message.Voice.FileId, AttachmentType.Voice, message.Voice, Name, null)
                    };
                case MessageType.Document:
                    return new[]
                    {
                        new InAttachment(message.Document.FileId, AttachmentType.Document, message.Document, Name,
                            null)
                    };
                case MessageType.Sticker:
                    return new[]
                    {
                        new InAttachment(message.Sticker.FileId, AttachmentType.Sticker, message.Sticker, Name,
                            null)
                    };
                case MessageType.VideoNote:
                    return new[]
                    {
                        new InAttachment(message.VideoNote.FileId, AttachmentType.Video, message.VideoNote, Name,
                            null)
                    };
                default:
                    return new InAttachment[0];
            }
        }
    }
}