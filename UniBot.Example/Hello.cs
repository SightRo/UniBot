using System.Threading.Tasks;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;
using UniBot.Core.Models;

namespace UniBot.Example
{
    public class Hello : CommandBase
    {
        public override string Name { get; } = "Hello";
        public override string? Description { get; } = "None.";
        public override async Task Execute(UpdateContext context)
        {
            var messenger = context.Messenger;
            await messenger.SendMessage(context.Chat.Id, new OutMessage("Hello, Bastard!"));
        }
    }
}