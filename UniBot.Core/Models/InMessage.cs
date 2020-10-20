using System;
using System.Collections.Immutable;
using UniBot.Core.Models.Attachments;

namespace UniBot.Core.Models
{
    public class InMessage : InModelBase<long>
    {
        public DateTime Date { get; set; }
        public long SenderId { get; set; }
        public long ChatId { get; set; }
        public string? Text { get; set; }
        public InMessage? Reply { get; set; }
        public ImmutableList<InMessage> Forwarded { get; set; } = ImmutableList<InMessage>.Empty;
        public ImmutableList<InAttachment> Attachments { get; set; } = ImmutableList<InAttachment>.Empty;
    }
}