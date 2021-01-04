using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using UniBot.Core.Models;
using UniBot.Core.Models.Attachments;
using UniBot.Core.Models.Keyboards;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;
using Chat = UniBot.Core.Models.Chat;
using User = UniBot.Core.Models.User;
using VkMessage = VkNet.Model.Message;
using VkAttachment = VkNet.Model.Attachments.Attachment;
using VkChat = VkNet.Model.Conversation;
using VkPhoto = VkNet.Model.Attachments.Photo;
using VkGroup = VkNet.Model.Group;
using VkUser = VkNet.Model.User;

namespace UniBot.AspNetCore.Vkontakte
{
    internal static class VkConverter
    {
        public static InMessage? ToInMessage(VkMessage? message)
        {
            if (message == null)
                return null;

            // Need to explicitly declare type to avoid warning.
            ImmutableList<InMessage> forwardedMessages = message.ForwardedMessages
                .Where(m => m != null)
                .Select(ToInMessage)
                .ToImmutableList()!;

            var attachments = message.Attachments
                .Select(ToInAttachment)
                .ToImmutableList();

            return new InMessage(
                (long) message.Id!,
                VkConstants.Name,
                message.PeerId!.Value,
                message.FromId!.Value,
                message.Date!.Value)
            {
                Text = message.Text,
                Reply = ToInMessage(message.ReplyMessage),
                Forwarded = forwardedMessages,
                Attachments = attachments
            };
        }

        // Use only when sure it's chat is of type Chat 
        public static Chat? ToChat(long chatId, VkChat chat)
        {
            ChatType type = ChatType.Group;
            if (chat.Peer.Type == ConversationPeerType.Chat)
                type = ChatType.Group;
            if (chat.Peer.Type == ConversationPeerType.User)
                type = ChatType.Private;
            if (chat.Peer.Type == ConversationPeerType.Group)
                type = ChatType.Private;
            // Not sure what to do with this
            if (chat.Peer.Type == ConversationPeerType.Email)
                type = ChatType.Group;

            return new Chat(
                chatId,
                VkConstants.Name,
                chat.ChatSettings.Title,
                chat.ChatSettings.OwnerId,
                type)
            {
                Photos = ToInAttachment(chat.ChatSettings.Photo),
                PinnedMessage = ToInMessage(chatId, chat.ChatSettings.PinnedMessage)
            };
        }

        // Use only when sure it's chat is of type User 
        public static Chat? ToChat(long chatId, User? user)
        {
            if (user == null)
                return null;

            // Get avatar of user
            return new Chat(
                chatId,
                VkConstants.Name,
                user.LastName + user.FirstName ?? user.Username ?? "User",
                chatId,
                ChatType.Private);
        }

        public static InAttachment ToInAttachment(VkAttachment attachment)
        {
            (AttachmentType type, string url) res = (attachment.Instance switch
            {
                Audio audio => (AttachmentType.Audio, audio.Url.ToString()),
                AudioMessage voice => (AttachmentType.Audio, voice.LinkOgg.ToString()),
                Document document => (AttachmentType.Document, document.Uri),
                Photo photo => (AttachmentType.Photo, photo.Sizes.Last().Url.ToString()),
                Sticker sticker => (AttachmentType.Sticker, sticker.ImagesWithBackground.Last().Url.ToString()),
                Video _ => (AttachmentType.Video, null),
                _ => (AttachmentType.Unknown, null)
            })!;
            return new InAttachment(attachment.ToString(), VkConstants.Name, res.type, attachment, res.url);
        }

        public static ImmutableList<InAttachment> ToInAttachment(VkPhoto photo)
        {
            var temp = ImmutableList.Create
            (
                photo.Photo50,
                photo.Photo75,
                photo.Photo100,
                photo.Photo130,
                photo.Photo200,
                photo.Photo604,
                photo.Photo807,
                photo.Photo1280,
                photo.Photo2560
            );

            return temp.Select(u =>
                    new InAttachment(
                        photo.ToString(),
                        VkConstants.Name,
                        AttachmentType.Photo,
                        photo,
                        u.ToString()))
                .ToImmutableList();
        }

        public static MessageKeyboard? ToVkKeyboard(InlineKeyboard? keyboard)
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

        public static User ToUser(VkGroup group)
        {
            return new User(
                group.Id,
                VkConstants.Name,
                false)
            {
                Username = group.ScreenName,
                FirstName = group.Name
            };
        }

        public static User ToUser(VkUser user)
        {
            return new User(
                user.Id,
                VkConstants.Name,
                true)
            {
                Username = user.ScreenName,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public static MessageKeyboard? ToVkKeyboard(ReplyKeyboard? keyboard)
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

        private static InMessage? ToInMessage(long chatId, PinnedMessage? message)
        {
            if (message == null)
                return null;

            ImmutableList<InMessage> forwardedMessages = message.ForwardMessages
                .Where(m => m != null)
                .Select(ToInMessage)
                .ToImmutableList()!;

            var attachments = message.Attachments.Select(ToInAttachment).ToImmutableList();

            return new InMessage(
                message.Id,
                VkConstants.Name,
                chatId,
                message.FromId,
                message.Date)
            {
                Text = message.Text,
                Forwarded = forwardedMessages,
                Attachments = attachments
            };
        }
    }
}