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
using User = UniBot.Core.Models.User;
using TgMessage = Telegram.Bot.Types.Message;
using TgUser = Telegram.Bot.Types.User;
using TgChat = Telegram.Bot.Types.Chat;

namespace UniBot.Telegram
{
    public class TgMessenger : IMessenger
    {
        private readonly ITelegramBotClient _api;
        private readonly TgSettings _settings;
        public string Name => Constants.Name;

        public TgMessenger(ITelegramBotClient api, TgSettings settings)
        {
            _api = api;
            _settings = settings;
        }
        
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