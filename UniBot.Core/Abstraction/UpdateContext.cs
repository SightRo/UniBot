using UniBot.Core.Models;

namespace UniBot.Core.Abstraction
{
    public class UpdateContext
    {
        public UpdateContext(IMessenger messenger, Chat chat, User sender, InMessage? message = null)
            => (Messenger, Chat, Sender, Message) = (messenger, chat, sender, message);
        
        public IMessenger Messenger { get; init; }
        public Chat Chat { get; init; }
        public User Sender { get; init; }
        public InMessage? Message { get; init; }

        public string Source => Messenger.Name;
    }
}