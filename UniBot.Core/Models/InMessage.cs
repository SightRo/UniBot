using System;
using UniBot.Core.Models.Attachments;

namespace UniBot.Core.Models
{
    public class InMessage : InModelBase<long>
    {
        public DateTime Date { get; set; }
        public User Sender { get; set; } = null!;
        public Chat Chat { get; set; } = null!;
        public string? Text { get; set; }
        public InMessage? Reply { get; set; }
        public InMessage[] Forwarded { get; set; } = new InMessage[0];
        public InAttachment[] Attachments { get; set; } = new InAttachment[0];
    }
}