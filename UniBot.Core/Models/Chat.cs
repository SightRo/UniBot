using System.Collections.Immutable;
using UniBot.Core.Models.Attachments;

namespace UniBot.Core.Models
{
    public class Chat : InModelBase<long>
    {
        public string Title { get; set; } = null!;
        public long OwnerId { get; set; }
        public ChatType Type { get; set; }
        public ImmutableList<InAttachment> Photos { get; set; } = ImmutableList<InAttachment>.Empty;
        public InMessage? PinnedMessage { get; set; }
    }
}