using System;

namespace UniBot.Core.Models.Attachments
{
    public class InAttachment : InModelBase<string>
    {
        public InAttachment(
            string id,
            string messengerSource,
            AttachmentType type,
            object? originalAttachment,
            string? urlSource)
            : base(id, messengerSource)
        {
            Type = type;
            OriginalAttachment = originalAttachment;
            UrlSource = urlSource;
        }

        public AttachmentType Type { get; }
        public Object? OriginalAttachment { get; }
        public string? UrlSource { get; }
    }
}