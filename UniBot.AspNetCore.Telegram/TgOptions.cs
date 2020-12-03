using UniBot.Core.Options;

namespace UniBot.AspNetCore.Telegram
{
    public class TgOptions : MessengerOptions
    {
        public Socks5? Socks5 { get; init; }
    }
}