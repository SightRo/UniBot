using UniBot.Core.Models;

namespace UniBot.Core.Abstraction
{
    public class UpdateContext
    {
        public UpdateContext(IMessenger messenger, User sender, Chat chat, InMessage? message = null)
            => (Messenger, Chat, Sender, Message) = (messenger, chat, sender, message);
        
        public IMessenger Messenger { get; }
        public Chat Chat { get; }
        public User Sender { get; }
        public InMessage? Message { get; }
        
        public string Source => Messenger.Name;
    }
}