using System.Reflection;
using UniBot.Core.Actions;

namespace UniBot.Core.AspNetCore
{
    public interface IBotBuilder
    {
        IBotBuilder DetectCommands();

        IBotBuilder AddCommand<TCommand>()
            where TCommand : CommandBase;

        IBotBuilder DetectMessengerImplementations();
        IBotBuilder AddMessenger(Assembly assembly);
    }
}