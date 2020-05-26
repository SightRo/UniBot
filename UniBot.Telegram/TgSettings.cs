using UniBot.Core.Settings;

namespace UniBot.Telegram
{
    public class TgSettings : SettingsBase
    {
        public Socks5? Socks5 { get; set; }
    }
}