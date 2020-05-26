using System.IO;
using Telegram.Bot.Types;
using UniBot.Core.Models.Attachments;

namespace UniBot.Telegram
{
    public static class FileAttachmentExtension
    {
        public static InputMedia ToTgMedia(this FileAttachment attachment)
            => new InputMedia(new MemoryStream(attachment.Data), attachment.FullName);

        public static InputMediaPhoto ToTgPhoto(this FileAttachment attachment)
            => new InputMediaPhoto(new InputMedia(new MemoryStream(attachment.Data), attachment.FullName));

        public static InputMediaVideo ToTgVideo(this FileAttachment attachment)
            => new InputMediaVideo(new InputMedia(new MemoryStream(attachment.Data), attachment.FullName));
    }
}