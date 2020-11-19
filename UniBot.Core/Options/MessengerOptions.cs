namespace UniBot.Core.Options
{
    // TODO Make validation.
    public class MessengerOptions
    {
        public string Token { get; init; } = null!;
        public long BotOwnerId { get; init; } = 0;
        public long[] BotAdminIds { get; init; } = new long[0];
    }
}