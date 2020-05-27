using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniBot.Core.Settings;

namespace UniBot.Core.Abstraction
{
    public interface IStartUpManager
    {
        void Init(IBot bot, IServiceCollection services, out IMessenger messenger, out SettingsBase settings);
    }
}
