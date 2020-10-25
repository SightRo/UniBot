using System.Threading.Tasks;
using UniBot.Core.Abstraction;

namespace UniBot.Core.Actions
{
    public interface IAction
    {
        Task Execute(UpdateContext context);
        bool CanExecute(UpdateContext context);
    }
}