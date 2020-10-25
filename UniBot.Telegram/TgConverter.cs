using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UniBot.Core.Models;
using UniBot.Core.Models.Attachments;
using UniBot.Core.Models.Keyboards;
using Chat = UniBot.Core.Models.Chat;
using ChatType = UniBot.Core.Models.ChatType;
using User = UniBot.Core.Models.User;
using TgMessage = Telegram.Bot.Types.Message;
using TgUser = Telegram.Bot.Types.User;
using TgChat = Telegram.Bot.Types.Chat;
using TgChatType = Telegram.Bot.Types.Enums.ChatType;

namespace UniBot.Telegram
{
    internal static class TgConverter
    {
        public static InMessage? ToMessage(TgMessage? message)
        {
            if (message == null)
                return null;


            return new InMessage(
                message.MessageId,
                TgConstants.Name,
                message.Date,
                message.From.Id,
                message.Chat.Id,
                message.Text ?? message.Caption,
                ToMessage(message.ReplyToMessage),
                null,
                ExtractAttachments(message));
        }

        public static Chat ToChat(TgChat chat, long ownerId)
        {
            ImmutableList<InAttachment>? photos = null;
            if (chat.Photo != null)
            {
                photos = ImmutableList.Create(
                    new InAttachment(chat.Photo.SmallFileId, TgConstants.Name, AttachmentType.Photo, null, null),
                    new InAttachment(chat.Photo.BigFileId, TgConstants.Name, AttachmentType.Photo, null, null)
                );
            }

            var type = chat.Type switch
            {
                TgChatType.Private => ChatType.Private,
                TgChatType.Channel => ChatType.Community,
                _ => ChatType.Group
            };


            return new Chat(
                chat.Id,
                TgConstants.Name,
                chat.Title ?? chat.Username,
                ownerId,
                type,
                photos,
                ToMessage(chat.PinnedMessage));
        }

        public static User ToUser(TgChat chat)
        {
            return new User(
                chat.Id,
                TgConstants.Name,
                chat.Username,
                chat.FirstName,
                chat.LastName,
                true);
        }

        public static User ToUser(TgUser user)
        {
            return new User(
                user.Id,
                TgConstants.Name,
                user.Username,
                user.FirstName,
                user.LastName,
                !user.IsBot);
        }

        public static IReplyMarkup? ToTgKeyboard(InlineKeyboard? keyboard)
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

        public static IReplyMarkup? ToTgKeyboard(ReplyKeyboard? keyboard)
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

        public static InputMedia ToTgMedia(IOutAttachment attachment)
        {
            var data = attachment.GetStream();
            return new InputMedia(data, attachment.FullName);
        }

        public static InputMediaPhoto ToTgPhoto(IOutAttachment attachment)
            => new InputMediaPhoto(ToTgMedia(attachment));


        public static InputMediaVideo ToTgVideo(IOutAttachment attachment)
            => new InputMediaVideo(ToTgMedia(attachment));

        private static ImmutableList<InAttachment> ExtractAttachments(TgMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Photo:
                    return message.Photo
                        .Select(p => new InAttachment(
                            p.FileId,
                            TgConstants.Name,
                            AttachmentType.Photo,
                            p,
                            null))
                        .ToImmutableList();


                case MessageType.Audio:
                    return ImmutableList.Create(
                        new InAttachment(
                            message.Audio.FileId,
                            TgConstants.Name,
                            AttachmentType.Audio,
                            message.Audio,
                            null));

                case MessageType.Video:
                    return ImmutableList.Create(
                        new InAttachment(
                            message.Video.FileId,
                            TgConstants.Name,
                            AttachmentType.Video,
                            message.Video,
                            null));

                case MessageType.Voice:
                    return ImmutableList.Create(
                        new InAttachment(
                            message.Voice.FileId,
                            TgConstants.Name,
                            AttachmentType.Voice,
                            message.Voice,
                            null));

                case MessageType.Document:
                    return ImmutableList.Create(
                        new InAttachment(
                            message.Document.FileId,
                            TgConstants.Name,
                            AttachmentType.Document,
                            message.Document,
                            null));

                case MessageType.Sticker:
                    return ImmutableList.Create(
                        new InAttachment(
                            message.Sticker.FileId,
                            TgConstants.Name,
                            AttachmentType.Sticker,
                            message.Sticker,
                            null));

                case MessageType.VideoNote:
                    return ImmutableList.Create(
                        new InAttachment(
                            message.VideoNote.FileId,
                            TgConstants.Name,
                            AttachmentType.Video,
                            message.VideoNote,
                            null));
                default:
                    return ImmutableList<InAttachment>.Empty;
            }
        }
    }
}