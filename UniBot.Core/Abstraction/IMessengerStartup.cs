using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Settings;

namespace UniBot.Core.Abstraction
{
    public interface IMessengerStartup
    {
        void Init(IBot bot, IServiceCollection services, out IMessenger messenger, out SettingsBase settings);
    }
}
