using System;
using System.Threading.Tasks;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;
using UniBot.Core.Models;

namespace UniBot.Example
{
    public class Delay : CommandBase
    {
        public override string Name { get; } = "Delay";
        public override string? Description { get; } = null;
        public override async Task Execute(UpdateContext context)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await context.Messenger.SendMessage(context.Chat.Id, new OutMessage(context.Message.Text));
        }
    }
}