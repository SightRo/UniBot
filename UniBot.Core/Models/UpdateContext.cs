using UniBot.Core.Abstraction;

namespace UniBot.Core.Models
{
    public class UpdateContext
    {
        public UpdateContext(IMessenger messenger, Chat chat, User sender, InMessage? message = null)
            => (Messenger, Chat, Sender, Message) = (messenger, chat, sender, message);
        
        public IMessenger Messenger { get; }
        public Chat Chat { get; }
        public User Sender { get; }
        public InMessage? Message { get; }

        public string Source => Messenger.Name;
    }
}