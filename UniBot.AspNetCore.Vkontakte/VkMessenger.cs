using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UniBot.Core.Abstraction;
using UniBot.Core.Helpers;
using UniBot.Core.Models;
using UniBot.Core.Models.Attachments;
using UniBot.Core.Models.Keyboards;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using Chat = UniBot.Core.Models.Chat;
using User = UniBot.Core.Models.User;


namespace UniBot.AspNetCore.Vkontakte
{
    public class VkMessenger : IMessenger
    {
        private readonly IVkApi _api;
        private readonly HttpClient _client;

        public VkMessenger(IVkApi api, string confirmationCode, HttpClient? client = null)
        {
            _api = api;
            ConfirmationCode = confirmationCode;
            _client = client ?? new HttpClient();
        }

        public string ConfirmationCode { get; }

        public string Name => VkConstants.Name;

        public async Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message)
        {
            var attachments = new List<MediaAttachment>(message.Attachments.Count);

            foreach (var attachment in message.Attachments)
            {
                MediaAttachment vkAttachment = attachment.Type switch
                {
                    AttachmentType.Audio => await UploadAudio(chatId, attachment).ConfigureAwait(false),
                    AttachmentType.Voice => await UploadAudio(chatId, attachment).ConfigureAwait(false),
                    AttachmentType.Photo => await UploadPhoto(chatId, attachment).ConfigureAwait(false),
                    _ => await UploadDocument(chatId, attachment).ConfigureAwait(false)
                };
                attachments.Add(vkAttachment);
            }

            var result = new List<long>
            {
                await _api.Messages.SendAsync(new MessagesSendParams
                {
                    PeerId = chatId,
                    RandomId = DateTime.Now.Millisecond,
                    Message = message.Text,
                    Keyboard = message.Keyboard switch
                    {
                        InlineKeyboard inlineKeyboard => VkConverter.ToVkKeyboard(inlineKeyboard),
                        ReplyKeyboard replyKeyboard => VkConverter.ToVkKeyboard(replyKeyboard),
                        _ => message.RemoveReplyKeyboard
                            ? new MessageKeyboard
                                {Buttons = new IEnumerable<MessageKeyboardButton>[0]}
                            : null
                    },
                    Attachments = attachments
                }).ConfigureAwait(false)
            };

            return result;
        }

        public async Task<bool> DeleteMessage(long chatId, long messageId)
        {
            var deletedIds = await _api.Messages.DeleteAsync(new[] {(ulong) messageId}, groupId: (ulong) chatId)
                .ConfigureAwait(false);
            return deletedIds.First().Value;
        }


        public async Task<bool> EditMessage(long chatId, long messageId, OutMessage message)
        {
            var success = await _api.Messages
                .EditAsync(new MessageEditParams {PeerId = messageId, Message = message.Text}).ConfigureAwait(false);
            return success;
        }


        public async Task<MemoryAttachment?> DownloadAttachment(InAttachment attachment)
        {
            if (attachment.UrlSource == null)
                return null;

            var data = await _client.GetByteArrayAsync(attachment.UrlSource).ConfigureAwait(false);
            string name = attachment.Type switch
            {
                AttachmentType.Audio => $"Audio{DateTime.Now.Ticks}.mp3",
                AttachmentType.Voice => $"Voice{DateTime.Now.Ticks}.ogg",
                AttachmentType.Document =>
                    $"Document{DateTime.Now.Ticks}.{((Document) attachment.OriginalAttachment).Ext}",
                AttachmentType.Photo => $"Photo{DateTime.Now.Ticks}.jpg",
                AttachmentType.Sticker => $"Sticker{DateTime.Now.Ticks}.jpg",
                _ => $"UnknownFileType{DateTime.Now.Millisecond}"
            };

            return AttachmentFactory.CreateMemoryAttachment(name, data, attachment.Type);
        }

        public async Task<User?> GetUser(long userId)
        {
            var user = (userId > 0) switch
            {
                true => await GetUserFromVk(userId).ConfigureAwait(false),
                false => await GetGroupFromVk(userId).ConfigureAwait(false)
            };

            return user;
        }

        public async Task<Chat?> GetChat(long chatId)
        {
            // Fixed shit with admin status, but not sure
            var chats = await _api.Messages.GetConversationsByIdAsync(new[] {chatId}).ConfigureAwait(false);

            if (chats.Count != 1)
            {
                if (!await IsBotAdmin(chatId).ConfigureAwait(false))
                {
                    // Todo Make configurable behavior for those kind of cases
                    var message = new OutMessage {Text = "Бот не может работать корректно без прав администратора."};
                    await SendMessage(chatId, message).ConfigureAwait(false);
                }

                return null;
            }

            var chat = chats.Items.First();

            if (chat.Peer.Type == ConversationPeerType.User)
            {
                var user = await GetUser(chatId).ConfigureAwait(false);
                return VkConverter.ToChat(chatId, user);
            }

            if (chat.Peer.Type == ConversationPeerType.Chat)
                return VkConverter.ToChat(chatId, chat);

            return null;
        }

        public object GetNativeObject()
        {
            return _api;
        }

        private async Task<Document> UploadDocument(long chatId, IOutAttachment attachment)
        {
            var server = await _api.Docs.GetMessagesUploadServerAsync(chatId, DocMessageType.Doc)
                .ConfigureAwait(false);
            var response = await UploadFile(server.UploadUrl, attachment.GetByteArray(), attachment.FullName)
                .ConfigureAwait(false);
            var documents = await _api.Docs.SaveAsync(response, attachment.FullName, null)
                .ConfigureAwait(false);

            if (documents.Count != 1)
                throw new Exception($"Error while loading document to {Name}");

            return (Document) documents[0].Instance;
        }

        private async Task<Photo> UploadPhoto(long chatId, IOutAttachment attachment)
        {
            var server = await _api.Photo.GetMessagesUploadServerAsync(chatId)
                .ConfigureAwait(false);
            var response = await UploadFile(server.UploadUrl, attachment.GetByteArray(), attachment.FullName)
                .ConfigureAwait(false);
            var photos = await _api.Photo.SaveMessagesPhotoAsync(response)
                .ConfigureAwait(false);

            if (photos.Count != 1)
                throw new Exception($"Error while loading photo to {Name}");

            return photos[0];
        }

        private async Task<AudioMessage> UploadAudio(long chatId, IOutAttachment attachment)
        {
            var server = await _api.Docs.GetMessagesUploadServerAsync(chatId, DocMessageType.AudioMessage)
                .ConfigureAwait(false);
            var response = await UploadFile(server.UploadUrl, attachment.GetByteArray(), attachment.Name)
                .ConfigureAwait(false);
            var documents = await _api.Docs.SaveAsync(response, attachment.Name, null)
                .ConfigureAwait(false);

            if (documents.Count != 1)
                throw new Exception($"Error while loading audio to {Name}");

            return (AudioMessage) documents[0].Instance;
        }

        private async Task<User?> GetUserFromVk(long id)
        {
            var users = await _api.Users.GetAsync(new[] {id}, ProfileFields.ScreenName).ConfigureAwait(false);
            if (users.Count != 1)
                return null;

            var user = users.First();

            return VkConverter.ToUser(user);
        }

        private async Task<User?> GetGroupFromVk(long id)
        {
            var groups = await _api.Groups.GetByIdAsync(new string[0], id.ToString(), GroupsFields.Status)
                .ConfigureAwait(false);

            if (groups.Count != 1)
                return null;

            var group = groups.First();

            return VkConverter.ToUser(group);
        }

        private async Task<bool> IsBotAdmin(long chatId)
        {
            try
            {
                await _api.Messages.GetConversationMembersAsync(chatId, new string[0]).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> UploadFile(string url, byte[] data, string fileName)
        {
            var requestContent = new MultipartFormDataContent();
            var dataContent = new ByteArrayContent(data);
            dataContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            requestContent.Add(dataContent, "file", fileName);

            var response = await _client.PostAsync(url, requestContent).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return Encoding.ASCII.GetString(responseJson);
        }
    }
}