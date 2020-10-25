using System.Collections.Immutable;
using UniBot.Core.Models.Attachments;

namespace UniBot.Core.Models
{
    public class Chat : InModelBase<long>
    {
        public Chat(
            long id,
            string messengerSource,
            string title,
            long ownerId,
            ChatType type,
            ImmutableList<InAttachment>? photos,
            InMessage? pinnedMessage)
            : base(id, messengerSource)
        {
            Title = title;
            OwnerId = ownerId;
            Type = type;
            Photos = photos ?? ImmutableList<InAttachment>.Empty;
            PinnedMessage = pinnedMessage;
        }

        public string Title { get; }
        public long OwnerId { get; }
        public ChatType Type { get; }
        public ImmutableList<InAttachment> Photos { get; }
        public InMessage? PinnedMessage { get; }
    }
}