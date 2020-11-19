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

        public string Title { get; init; }
        public long OwnerId { get; init; }
        public ChatType Type { get; init; }
        public ImmutableList<InAttachment> Photos { get; init; }
        public InMessage? PinnedMessage { get; init; }
    }
}