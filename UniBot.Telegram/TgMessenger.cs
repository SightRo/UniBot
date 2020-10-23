using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
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
using TgChatType = Telegram.Bot.Types.Enums.ChatType;

namespace UniBot.Telegram
{
    public class TgMessenger : IMessenger
    {
        private readonly ITelegramBotClient _api;
        private readonly TgOptions _options;

        public TgMessenger(ITelegramBotClient api, TgOptions options)
        {
            _api = api;
            _options = options;
        }

        public string Name => TgConstants.Name;


        public async Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message)
        {
            var result = new List<long>(message.Attachments.Count + 1);

            GroupAttachments(message.Attachments, out var photos, out var videos, out var others);
            var photoMediaGroup = photos.Select(TgConverter.ToTgPhoto);
            var videoMediaGroup = videos.Select(TgConverter.ToTgVideo);

            if (photos.Count > 0)
                result.AddRange((await _api.SendMediaGroupAsync(photoMediaGroup, chatId))
                    .Select(a => (long) a.MessageId));
            if (videos.Count > 0)
                result.AddRange((await _api.SendMediaGroupAsync(videoMediaGroup, chatId))
                    .Select(a => (long) a.MessageId));

            foreach (var attachment in others)
            {
                switch (attachment.AttachmentType)
                {
                    case AttachmentType.Audio:
                        result.Add((await _api.SendAudioAsync(chatId, TgConverter.ToTgMedia(attachment))).MessageId);
                        break;
                    case AttachmentType.Voice:
                        result.Add((await _api.SendVoiceAsync(chatId, TgConverter.ToTgMedia(attachment))).MessageId);
                        break;
                    default:
                        result.Add((await _api.SendDocumentAsync(chatId, TgConverter.ToTgMedia(attachment))).MessageId);
                        break;
                }
            }

            var keyboard = message.Keyboard switch
            {
                InlineKeyboard inlineKeyboard => TgConverter.ToTgKeyboard(inlineKeyboard),
                ReplyKeyboard replyKeyboard => TgConverter.ToTgKeyboard(replyKeyboard),
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

        // Todo Make it work as expected
        // Need to identify type of message and then call method with specified message
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

        public async Task<MemoryAttachment?> DownloadAttachment(InAttachment inAttachment)
        {
            var file = await _api.GetFileAsync(inAttachment.Id);
            var url = $"https://api.telegram.org/file/bot{_options.Token}/{file.FilePath}";
            // Todo Change Attachment system.
            using var client = new HttpClient();
            var response = await client.GetByteArrayAsync(url);

            // Todo Refactor this.
            int index = file.FilePath.LastIndexOf('/');
            var fullName = file.FilePath.Substring(index + 1);

            return AttachmentFactory.CreateMemoryAttachment(fullName, response, inAttachment.Type);
        }

        public async Task<User?> GetUser(long userId)
        {
            var user = await _api.GetChatAsync(userId);
            return TgConverter.ToUser(user);
        }

        public async Task<Chat?> GetChat(long chatId)
        {
            var chat = await _api.GetChatAsync(chatId);

            var ownerId = chat.Type switch
            {
                TgChatType.Private => chat.Id,
                _ => (await _api.GetChatAdministratorsAsync(chatId))
                    .First(u => u.Status == ChatMemberStatus.Creator).User.Id
            };

            return TgConverter.ToChat(chat, ownerId);
        }

        public object GetNativeObject()
            => _api;

        private void GroupAttachments(ICollection<IOutAttachment> attachments, out List<IOutAttachment> photos, out List<IOutAttachment> videos, out List<IOutAttachment> others)
        {
            photos = new List<IOutAttachment>();
            videos = new List<IOutAttachment>();
            others = new List<IOutAttachment>();

            foreach (var attachment in attachments)
            {
                switch (attachment.AttachmentType)
                {
                    case AttachmentType.Photo:
                        photos.Add(attachment);
                        break;
                    case AttachmentType.Video:
                        videos.Add(attachment);
                        break;
                    default:
                        others.Add(attachment);
                        break;
                }
            }
        }
    }
}