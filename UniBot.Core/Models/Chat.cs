using System.Collections.Immutable;
using UniBot.Core.Annotations;
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
            ChatType type)
            : base(id, messengerSource)
        {
            Title = title;
            OwnerId = ownerId;
            Type = type;
            //Permissions = permissions;
        }

        public string Title { get; }
        public long OwnerId { get; }
        public ChatType Type { get; }
        //public ChatPermissions Permissions { get; }
        public InMessage? PinnedMessage { get; init; }
        public ImmutableList<InAttachment> Photos { get; init; } = ImmutableList<InAttachment>.Empty;
    }
}