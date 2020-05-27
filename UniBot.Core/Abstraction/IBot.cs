using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using UniBot.Core.Actions;

namespace UniBot.Core.Abstraction
{
    public interface IBot
    {
        ConcurrentDictionary<string, long> Owners { get; }
        ConcurrentDictionary<string, long[]> Admins { get; }
        ConcurrentDictionary<string, CommandBase> Commands { get; }
        
        IConfiguration Configuration { get; }
        
        void ProcessUpdate(UpdateContext context);
        IMessenger ResolveMessenger(string name);
    }
}