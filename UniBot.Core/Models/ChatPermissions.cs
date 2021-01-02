namespace UniBot.Core.Models
{
    public class ChatPermissions
    {
        public ChatPermissions(
            bool canSendMessages = false, 
            bool canPinMessage = false, 
            bool canDeleteMessages = false, 
            bool canEditMessages = false)
        {
            CanSendMessages = canSendMessages;
            CanPinMessage = canPinMessage;
            CanDeleteMessages = canDeleteMessages;
            CanEditMessages = canEditMessages;
        }

        public bool CanSendMessages { get; init; }
        public bool CanPinMessage { get; init; }
        public bool CanDeleteMessages { get; init; }
        public bool CanEditMessages { get; init; }
    }
}