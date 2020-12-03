namespace UniBot.Core.Abstraction
{
    public interface IMessengerStartup
    {
        void Init(IBot bot, out IMessenger messenger);
    }
}
