using System;
using System.Collections.Immutable;
using UniBot.Core.Models.Attachments;

namespace UniBot.Core.Models
{
    public class InMessage : InModelBase<long>
    {
        public InMessage(
            long id,
            string messengerSource,
            long chatId,
            long senderId,
            DateTime date)
            : base(id, messengerSource)
        {
            ChatId = chatId;
            SenderId = senderId;
            Date = date;
        }

        public long ChatId { get; }
        public long SenderId { get; }
        public DateTime Date { get; }
        public string? Text { get; init; }
        public InMessage? Reply { get; init; }
        public ImmutableList<InMessage> Forwarded { get; init; } = ImmutableList<InMessage>.Empty;
        public ImmutableList<InAttachment> Attachments { get; init; } = ImmutableList<InAttachment>.Empty;
    }
}