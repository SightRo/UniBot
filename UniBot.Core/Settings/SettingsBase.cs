namespace UniBot.Core.Settings
{
    // TODO Make validation.
    public class SettingsBase
    {
        public bool IsEnabled { get; set; } = true;
        public string Token { get; set; } = null!;
        public long BotOwnerId { get; set; } = 0;
        public long[] BotAdminIds { get; set; } = new long[0];
    }
}