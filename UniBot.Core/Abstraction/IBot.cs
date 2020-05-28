using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using UniBot.Core.Actions;
using UniBot.Core.Settings;

namespace UniBot.Core.Abstraction
{
    public interface IBot
    {
        IReadOnlyDictionary<string, long> Owners { get; }
        IReadOnlyDictionary<string, long[]> Admins { get; }
        IReadOnlyDictionary<string, CommandBase> Commands { get; }
        IReadOnlyDictionary<string, IMessenger> Messengers { get; }
        BotSettings Settings { get; }
        
        void ProcessUpdate(UpdateContext context);
        IMessenger ResolveMessenger(string name);
    }
}