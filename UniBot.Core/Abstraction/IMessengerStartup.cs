using Microsoft.Extensions.DependencyInjection;

namespace UniBot.Core.Abstraction
{
    public interface IMessengerStartup
    {
        void Init(Bot bot, IServiceCollection services, out IMessenger messenger);
    }
}
