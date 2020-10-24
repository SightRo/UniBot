using UniBot.Core.Actions;

namespace UniBot.Core.Abstraction
{
    public class JobItem
    {
        public JobItem(IAction action, UpdateContext context)
        {
            Action = action;
            Context = context;
        }

        public IAction Action { get; set; }
        public UpdateContext Context { get; set; }
    }
}