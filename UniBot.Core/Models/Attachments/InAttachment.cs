using System;

namespace UniBot.Core.Models.Attachments
{
    public class InAttachment : InModelBase<string>
    {
        public InAttachment(string id, AttachmentType type, Object attachment, string messenger, string? urlSource)
            : base(id, messenger)
        {
            Type = type;
            OriginalAttachment = attachment;
            UrlSource = urlSource;
        }

        public AttachmentType Type { get; }
        public Object OriginalAttachment { get; }
        public string? UrlSource { get; }
    }
}