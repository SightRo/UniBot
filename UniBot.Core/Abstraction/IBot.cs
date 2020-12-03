using System.Reflection;
using UniBot.Core.Actions;
using UniBot.Core.Models;
using UniBot.Core.Options;

namespace UniBot.Core.Abstraction
{
    public interface IBot
    {
        BotOptions BotOptions { get; }
        void ProcessUpdate(UpdateContext context);
        CommandBase? GetCommand(string commandName);
        IMessenger ResolveMessenger(string name);
        void InitializeMessengers();
        void AddMessengerImplementation(Assembly assembly);

        void AddCommand<TCommand>(TCommand command)
            where TCommand : CommandBase;
    }
}