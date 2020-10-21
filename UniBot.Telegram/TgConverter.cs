using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UniBot.Core.Abstraction;
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
    public static class TgConverter
    {
        public static InMessage? ToMessage(TgMessage? message)
        {
            if (message == null)
                return null;

            return new InMessage
            {
                Id = message.MessageId,
                Date = message.Date,
                SenderId = message.From.Id,
                ChatId = message.Chat.Id,
                Text = message.Text ?? message.Caption,
                Reply = ToMessage(message.ReplyToMessage),
                //Todo Implement Forwarded messages. 
                //Forwarded = ImmutableList.Create<InMessage>(ToMessage())
                Attachments = ExtractAttachments(message)
            };
        }

        public static Chat ToChat(TgChat chat, long ownerId)
        {
            return new Chat
            {
                Id = chat.Id,
                Title = chat.Title ?? chat.Username,
                OwnerId = ownerId,
                Type = chat.Type switch
                {
                    TgChatType.Private => ChatType.Private,
                    TgChatType.Channel => ChatType.Community,
                    _ => ChatType.Group
                },
                Photos = ImmutableList.Create(
                    new InAttachment(chat.Photo.SmallFileId, TgConstants.Name, AttachmentType.Photo, null, null),
                    new InAttachment(chat.Photo.BigFileId, TgConstants.Name, AttachmentType.Photo, null, null)
                ),
                MessengerSource = TgConstants.Name
            };
        }

        public static User ToUser(TgChat chat)
        {
            return new User
            {
                Id = chat.Id,
                Username = chat.Username,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                IsHuman = true,
                MessengerSource = TgConstants.Name
            };
        }
        
        public static User ToUser(TgUser user)
        {
            return new User
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsHuman = !user.IsBot,
                MessengerSource = TgConstants.Name
            };
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

        //public static InAttachment ToAttachment()

        public static InputMedia ToTgMedia(FileAttachment attachment)
        {
            var data = attachment.File.OpenRead();
            return new InputMedia(data, attachment.FullName);
        }

        public static InputMediaPhoto ToTgPhoto(FileAttachment attachment)
            => new InputMediaPhoto(ToTgMedia(attachment));


        public static InputMediaVideo ToTgVideo(FileAttachment attachment)
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