namespace UniBot.Core.Abstraction
{
    public interface IMessengerStartup
    {
        void Init(Bot bot, out IMessenger messenger);
    }
}
