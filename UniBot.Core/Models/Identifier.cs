namespace UniBot.Core.Models
{
    public record Identifier<TId>(
        TId Id,
        string Messenger);
}