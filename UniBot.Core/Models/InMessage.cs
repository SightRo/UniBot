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

        public DateTime Date { get; }
        public long SenderId { get; }
        public long ChatId { get; }
        public string? Text { get; }
        public InMessage? Reply { get; }
        public ImmutableList<InMessage> Forwarded { get; }
        public ImmutableList<InAttachment> Attachments { get; }
    }
}