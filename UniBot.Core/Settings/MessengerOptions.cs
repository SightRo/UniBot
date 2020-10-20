namespace UniBot.Core.Settings
{
    // TODO Make validation.
    public class MessengerOptions
    {
        public string Token { get; set; } = null!;
        public long BotOwnerId { get; set; } = 0;
        public long[] BotAdminIds { get; set; } = new long[0];
    }
}