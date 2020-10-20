using UniBot.Core.Settings;

namespace UniBot.Telegram
{
    public class TgSettings : MessengerOptions
    {
        public Socks5? Socks5 { get; set; }
    }
}