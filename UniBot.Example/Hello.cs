using System.Threading.Tasks;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;
using UniBot.Core.Models;

namespace UniBot.Example
{
    public class Hello : Command
    {
        public override string Name { get; } = "Hello";
        public override string? Description { get; } = "None.";

        public override async Task Execute(UpdateContext context)
        {
            await context.Messenger.SendMessage(context.Chat.Id,
                new OutMessage($"Hello, {context.Sender.FirstName ?? context.Sender.Username ?? "Unknown User"}"));
        }
    }
}