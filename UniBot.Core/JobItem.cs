using UniBot.Core.Abstraction;
using UniBot.Core.Actions;

namespace UniBot.Core
{
    public class JobItem
    {
        public IAction Action { get; set; }
        public UpdateContext Context { get; set; }
    }
}