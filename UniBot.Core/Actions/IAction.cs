using System.Threading.Tasks;
using UniBot.Core.Abstraction;
using UniBot.Core.Models;

namespace UniBot.Core.Actions
{
    public interface IAction
    {
        Task Execute(UpdateContext context);
        bool CanExecute(UpdateContext context);
    }
}