using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
using VkMessage = VkNet.Model.Message;
using VkSticker = VkNet.Model.Attachments.Sticker;


namespace UniBot.Vkontakte
{
    public class VkMessenger : IMessenger
    {
        private readonly IVkApi _api;

        public VkMessenger(IVkApi api)
        {
            _api = api;
        }
        public string Name => Constants.Name;

        public async Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message)
        {
            var attachments = new List<MediaAttachment>(message.Attachments.Length);

            foreach (var attachment in message.Attachments)
            {
                MediaAttachment vkAttachment = attachment.AttachmentType switch
                {
                    AttachmentType.Audio => await UploadAudio(chatId, attachment),
                    AttachmentType.Voice => await UploadAudio(chatId, attachment),
                    AttachmentType.Photo => await UploadPhoto(chatId, attachment),
                    _ => await UploadDocument(chatId, attachment),
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
                        InlineKeyboard inlineKeyboard => ConvertInlineKeyboard(inlineKeyboard),
                        ReplyKeyboard replyKeyboard => ConvertReplyKeyboard(replyKeyboard),
                        _ => message.RemoveReplyKeyboard ? new MessageKeyboard() 
                        { Buttons = new IEnumerable<MessageKeyboardButton>[0] }
                            : null
                    },
                    Attachments = attachments
                })
            };

            return result;
        }

        public async Task<bool> DeleteMessage(long chatId, long messageId)
            => (await _api.Messages.DeleteAsync(new[] {(ulong) messageId}, groupId: (ulong) chatId)).First().Value;


        public Task<bool> EditMessage(long chatId, long messageId, OutMessage message)
            => _api.Messages.EditAsync(new MessageEditParams {PeerId = messageId, Message = message.Text});


        public async Task<FileAttachment?> DownloadAttachment(InAttachment attachment)
        {
            if (attachment.UrlSource == null)
                return null;

            using var client = new WebClient();
            var data = await client.DownloadDataTaskAsync(attachment.UrlSource);
            string name = attachment.Type switch
            {
                AttachmentType.Audio => $"Audio{DateTime.Now.Ticks}.mp3",
                AttachmentType.Voice => $"Voice{DateTime.Now.Ticks}.ogg",
                AttachmentType.Document => $"Document{DateTime.Now.Ticks}.{((Document)attachment.OriginalAttachment).Ext}",
                AttachmentType.Photo => $"Photo{DateTime.Now.Ticks}.jpg",
                AttachmentType.Sticker => $"Sticker{DateTime.Now.Ticks}.jpg",
                _ => $"UnknownFileType{DateTime.Now.Millisecond}"
            };
            
            return AttachmentFactory.CreateFileAttachment(name, data, attachment.Type);
        }

        public async Task<User?> GetUser(long userId)
        {
            var user = (userId > 0) switch
            {
                true => await GetUserFromVk(userId),
                false => await GetGroupFromVk(userId)
            };

            return user;
        }

        public async Task<Chat?> GetChat(long chatId)
        {
            // Fixed shit with admin status, but not sure
            var chats = await _api.Messages.GetConversationsByIdAsync(new[] {chatId});

            if (chats.Count != 1)
            {
                if (!await IsBotAdmin(chatId))
                {
                    var message = new OutMessage {Text = "Бот не может работать корректно без прав администратора."};
                    await SendMessage(chatId, message);
                }

                return null;
            }

            var chat = chats.Items.First();

            if (chat.Peer.Type == ConversationPeerType.User)
            {
                var user = await GetUser(chatId);
                var name = user!.FirstName + " " + user!.LastName;
                return new Chat
                {
                    Id = chatId,
                    Name = name,
                    Owner = chatId,
                    IsUserConversation = true,
                    MessengerSource = Name
                };
            }
            if (chat.Peer.Type == ConversationPeerType.Chat)
            {
                return new Chat
                {
                    Id = chatId,
                    Name = chat.ChatSettings.Title,
                    Owner = chat.ChatSettings.OwnerId,
                    IsUserConversation = false,
                    MessengerSource = Name
                };
            }

            return null;
        }

        private async Task<Document> UploadDocument(long chatId, FileAttachment attachment)
        {
            var server = await _api.Docs.GetMessagesUploadServerAsync(chatId, DocMessageType.Doc);
            var response = await UploadFile(server.UploadUrl, attachment.Data, attachment.FullName);
            var documents = await _api.Docs.SaveAsync(response, attachment.FullName, null);
            
            if (documents.Count != 1)
                throw new Exception($"Error while loading document to {Name}");

            return (Document)documents[0].Instance;
        }

        private async Task<Photo> UploadPhoto(long chatId, FileAttachment attachment)
        {
            var server = await _api.Photo.GetMessagesUploadServerAsync(chatId);
            var response = await UploadFile(server.UploadUrl, attachment.Data, attachment.FullName);
            var photos = await _api.Photo.SaveMessagesPhotoAsync(response);
            
            if (photos.Count != 1)
                throw new Exception($"Error while loading photo to {Name}");

            return photos[0];
        }
        
        private async Task<AudioMessage> UploadAudio(long chatId, FileAttachment attachment)
        {
            var server = await _api.Docs.GetMessagesUploadServerAsync(chatId, DocMessageType.AudioMessage);
            var response = await UploadFile(server.UploadUrl, attachment.Data, attachment.Name);
            var documents = await _api.Docs.SaveAsync(response, attachment.Name, null);
            
            if (documents.Count != 1)
                throw new Exception($"Error while loading audio to {Name}");

            return (AudioMessage)documents[0].Instance;
        }

        private MessageKeyboard? ConvertInlineKeyboard(InlineKeyboard keyboard)
        {
            if (keyboard == null)
                return null;
            
            var buttons = new List<List<MessageKeyboardButton>>(keyboard.Buttons.Count);
            for (int i = 0; i < keyboard.Buttons.Count; i++)
            {
                var line = keyboard.Buttons[i];
                buttons.Add(new List<MessageKeyboardButton>(line.Count));

                foreach (var button in line)
                    buttons[i].Add(ConvertInlineButton(button));
            }

            var result = new MessageKeyboard
            {
                Buttons = buttons,
                Inline = true
            };
            return result;
            
            MessageKeyboardButton ConvertInlineButton(InlineButton button)
            {
                return new MessageKeyboardButton
                {
                    Action = new MessageKeyboardButtonAction
                    {
                        Label = button.Text,
                        Link = button.Link != null ? new Uri(button.Link) : null,
                        Payload = JsonConvert.SerializeObject(button.CallbackData),
                        Type = button.Link == null ? KeyboardButtonActionType.Text : KeyboardButtonActionType.OpenLink
                    }
                };
            }
        }

        private MessageKeyboard? ConvertReplyKeyboard(ReplyKeyboard keyboard)
        {
            if (keyboard == null)
                return null;
            
            var buttons = new List<List<MessageKeyboardButton>>(keyboard.Buttons.Count);
            for (int i = 0; i < keyboard.Buttons.Count; i++)
            {
                var line = keyboard.Buttons[i];
                buttons.Add(new List<MessageKeyboardButton>(line.Count));

                foreach (var button in line)
                    buttons[i].Add(ConvertReplyButton(button));
            }
            
            var result = new MessageKeyboard
            {
                Buttons = buttons,
                Inline = false,
                OneTime = keyboard.OneTime
            };
            return result;

            MessageKeyboardButton ConvertReplyButton(ReplyButton button)
            {
                return new MessageKeyboardButton
                {
                    Action = new MessageKeyboardButtonAction
                    {
                        Label = button.Text,
                        Payload = JsonConvert.SerializeObject(button.CallbackData),
                        Type = KeyboardButtonActionType.Text
                    }
                };
            }
        }

        private async Task<User?> GetUserFromVk(long id)
        {
            var users = await _api.Users.GetAsync(new[] {id}, ProfileFields.ScreenName);
            if (users.Count != 1) return null;

            var user = users.First();

            return new User
            {
                Id = user.Id,
                Username = user.ScreenName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsHuman = true,
                MessengerSource = Name
            };
        }

        private async Task<User?> GetGroupFromVk(long id)
        {
            var groups = await _api.Groups.GetByIdAsync(new string[0], id.ToString(), GroupsFields.Status);

            if (groups.Count != 1) return null;

            var group = groups.First();

            return new User
            {
                Id = group.Id,
                Username = group.ScreenName,
                FirstName = group.Name,
                IsHuman = false,
                MessengerSource = Name
            };
        }

        private async Task<bool> IsBotAdmin(long chatId)
        {
            try
            {
                await _api.Messages.GetConversationMembersAsync(chatId, new string[0]);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> UploadFile(string url, byte[] data, string fileName)
        {
            using var client = new HttpClient();
            var requestContent = new MultipartFormDataContent();
            var dataContent = new ByteArrayContent(data);
            dataContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            requestContent.Add(dataContent, "file", fileName);
            
            var response = await client.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();

            return Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync());
        }
    }
}