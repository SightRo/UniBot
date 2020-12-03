using UniBot.Core.Actions;
using UniBot.Core.Models;

namespace UniBot.Core.Utils
{
    internal record JobItem(
        IAction Action,
        UpdateContext Context
    );
}