using System.Threading.Tasks;
using UniBot.Core.Abstraction;
using UniBot.Core.Models;

namespace UniBot.Core.Actions
{
    public abstract class CommandBase : IAction
    {
        public abstract string Name { get; }
        public abstract string? Description { get; }

        public abstract Task Execute(UpdateContext context);

        public virtual bool CanExecute(UpdateContext context)
            => true;
        
        public static bool TryParseCommand(string? text, out string commandName)
        {
            commandName = string.Empty;
            if (string.IsNullOrEmpty(text))
                return false;
            text = text.Trim();

            if (!text.StartsWith('/') || text.Length <= 1)
                return false;

            commandName = text.Substring(1, text.Length - 1);
            return true;
        }
    }
}