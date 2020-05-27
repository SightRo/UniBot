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
        ConcurrentDictionary<string, IAction> Actions { get; }
        
        IConfiguration Configuration { get; }
        
        Task ProcessUpdate(UpdateContext context);
        IMessenger ResolveMessenger(string name);
    }
}