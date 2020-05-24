using System.Threading.Tasks;
using UniBot.Core.Abstraction;

namespace UniBot.Core.Actions
{
    public abstract class CommandBase : IAction
    {
        public abstract string Name { get; }
        public abstract string? Description { get; }

        public abstract Task Execute(UpdateContext context);

        public virtual bool CanExecute(UpdateContext context)
            => true;
    }
}