using UniBot.Core.Abstraction;
using UniBot.Core.Actions;

namespace UniBot.Core.Utils
{
    internal record JobItem(
        IAction Action, 
        UpdateContext Context);
}