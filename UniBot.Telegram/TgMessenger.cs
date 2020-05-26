using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UniBot.Core.Abstraction;
using UniBot.Core.Helpers;
using UniBot.Core.Models;
using UniBot.Core.Models.Attachments;
using UniBot.Core.Models.Keyboards;
using Chat = UniBot.Core.Models.Chat;
using TgMessage = Telegram.Bot.Types.Message;
using TgUser = Telegram.Bot.Types.User;
using TgChat = Telegram.Bot.Types.Chat;
using User = UniBot.Core.Models.User;

namespace UniBot.Telegram
{
    // Doesn't interact with anything.
    // No way to get needed dependencies.            
    // So doesn't work for now because of reasons above.
    public class TgMessenger : IMessenger
    {
        private ITelegramBotClient _api = null!;
        private TgSettings _settings = null!;
        public string Name { get; } = "Telegram";

        public async Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message)
        {
            var result = new List<long>(message.Attachments.Length + 1);

            var (photos, videos, others) = ConvertAttachments(message.Attachments);

            if (photos.Count > 0)
                result.AddRange((await _api.SendMediaGroupAsync(photos, chatId)).Select(a => (long) a.MessageId));
            if (videos.Count > 0)
                result.AddRange((await _api.SendMediaGroupAsync(videos, chatId)).Select(a => (long) a.MessageId));

            foreach (var attachment in others)
            {
                switch (attachment.AttachmentType)
                {
                    case AttachmentType.Audio:
                        result.Add((await _api.SendAudioAsync(chatId, attachment.ToTgMedia())).MessageId);
                        break;
                    case AttachmentType.Voice:
                        result.Add((await _api.SendVoiceAsync(chatId, attachment.ToTgMedia())).MessageId);
                        break;
                    default:
                        result.Add((await _api.SendDocumentAsync(chatId, attachment.ToTgMedia())).MessageId);
                        break;
                }
            }

            var keyboard = message.Keyboard switch
            {
                InlineKeyboard inlineKeyboard => ConvertInlineKeyboard(inlineKeyboard),
                ReplyKeyboard replyKeyboard => ConvertReplyKeyboard(replyKeyboard),
                _ => message.RemoveReplyKeyboard ? new ReplyKeyboardRemove() : null
            };

            result.Add((await _api.SendTextMessageAsync(chatId, message.Text, replyMarkup: keyboard)).MessageId);

            return result;
        }

        public async Task<bool> DeleteMessage(long chatId, long messageId)
        {
            try
            {
                await _api.DeleteMessageAsync(chatId, (int) messageId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EditMessage(long chatId, long messageId, OutMessage message)
        {
            try
            {
                await _api.EditMessageTextAsync((int) chatId, (int) messageId, message.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<FileAttachment?> DownloadAttachment(InAttachment inAttachment)
        {
            var file = await _api.GetFileAsync(inAttachment.Id);
            var url = $"https://api.telegram.org/file/bot{_settings.Token}/{file.FilePath}";
            using var client = new WebClient();

            var data = await client.DownloadDataTaskAsync(url);

            // TODO Refactor this.
            int index = file.FilePath.LastIndexOf('/');
            var fullName = file.FilePath.Substring(index + 1);
            
            return AttachmentFactory.CreateFileAttachment(fullName, data, inAttachment.Type);
        }

        public async Task<User?> GetUser(long userId)
        {
            var user = await _api.GetChatAsync(userId);
            return new User
            {
                Id = userId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsHuman = true,
                MessengerSource = Name
            };
        }

        public async Task<Chat?> GetChat(long chatId)
        {
            var chat = await _api.GetChatAsync(chatId);

            var admins = await _api.GetChatAdministratorsAsync(chatId);
            var ownerId = admins.First(u => u.Status == ChatMemberStatus.Creator).User.Id;

            return new Chat
            {
                Id = chatId,
                Name = chat.Title ?? chat.Username,
                Owner = ownerId,
                IsUserConversation = chat.Type == ChatType.Private,
                MessengerSource = Name
            };
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

        private async Task<Chat> ConvertChat(TgChat chat, TgMessage message)
        {
            var isUserConversation = chat.Type == ChatType.Private;
            long owner = isUserConversation
                ? message.From.Id
                : (await _api.GetChatAdministratorsAsync(chat.Id))
                  .First(u => u.Status == ChatMemberStatus.Creator)
                  .User.Id;

            return new Chat
            {
                Id = chat.Id,
                Name = chat.Title ?? chat.Username,
                Owner = owner,
                IsUserConversation = isUserConversation,
                MessengerSource = Name
            };
        }

        private (List<IAlbumInputMedia> photos, List<IAlbumInputMedia> videos, List<FileAttachment> others) 
        ConvertAttachments(FileAttachment[] attachments)
        {
            List<IAlbumInputMedia> photos = new List<IAlbumInputMedia>();
            List<IAlbumInputMedia> videos = new List<IAlbumInputMedia>();
            List<FileAttachment> others = new List<FileAttachment>();

            foreach (var attachment in attachments)
            {
                switch (attachment.AttachmentType)
                {
                    case AttachmentType.Photo:
                        photos.Add(attachment.ToTgPhoto());
                        break;
                    case AttachmentType.Video:
                        videos.Add(attachment.ToTgVideo());
                        break;
                    default:
                        others.Add(attachment);
                        break;
                }
            }

            return (photos, videos, others);
        }

        private IReplyMarkup? ConvertInlineKeyboard(InlineKeyboard keyboard)
        {
            if (keyboard == null) 
                return null;

            var buttons = new List<List<InlineKeyboardButton>>(keyboard.Buttons.Count);
            for (var i = 0; i < keyboard.Buttons.Count; i++)
            {
                var line = keyboard.Buttons[i];
                buttons.Add(new List<InlineKeyboardButton>(line.Count));

                foreach (var button in line) 
                    buttons[i].Add(ConvertInlineButton(button));
            }

            var result = new InlineKeyboardMarkup(buttons);
            return result;
            
            InlineKeyboardButton ConvertInlineButton(InlineButton button)
            {
                return new InlineKeyboardButton
                {
                    Text = button.Text, Url = button.Link, CallbackData = button.CallbackData
                };
            }
        }

        private ReplyKeyboardMarkup? ConvertReplyKeyboard(ReplyKeyboard keyboard)
        {
            if (keyboard == null) 
                return null;

            var buttons = new List<List<KeyboardButton>>(keyboard.Buttons.Count);
            for (var i = 0; i < keyboard.Buttons.Count; i++)
            {
                var line = keyboard.Buttons[i];
                buttons.Add(new List<KeyboardButton>(line.Count));

                foreach (var button in line) 
                    buttons[i].Add(ConvertReplyButton(button));
            }

            var result = new ReplyKeyboardMarkup(buttons, true, keyboard.OneTime);
            return result;

            KeyboardButton ConvertReplyButton(ReplyButton button)
                => new KeyboardButton(button.Text);
        }
    }
}