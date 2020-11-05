using UniBot.Core.Options;

namespace UniBot.Telegram
{
    public class TgOptions : MessengerOptions
    {
        public Socks5? Socks5 { get; set; }
    }
}