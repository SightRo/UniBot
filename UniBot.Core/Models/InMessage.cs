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
            DateTime date,
            long senderId,
            long chatId,
            string? text,
            InMessage? reply,
            ImmutableList<InMessage>? forwarded,
            ImmutableList<InAttachment>? attachments)
            : base(id, messengerSource)
        {
            Date = date;
            SenderId = senderId;
            ChatId = chatId;
            Text = text;
            Reply = reply;
            Forwarded = forwarded ?? ImmutableList<InMessage>.Empty;
            Attachments = attachments ?? ImmutableList<InAttachment>.Empty;
        }

        public DateTime Date { get; init; }
        public long SenderId { get; init; }
        public long ChatId { get; init; }
        public string? Text { get; init; }
        public InMessage? Reply { get; init; }
        public ImmutableList<InMessage> Forwarded { get; init; }
        public ImmutableList<InAttachment> Attachments { get; init; }
    }
}