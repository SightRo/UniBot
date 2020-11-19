namespace UniBot.Telegram
{
    public class Socks5
    {
        public string Hostname { get; init; } = null!;
        public int Port { get; init; } = 0;
        public string? Username { get; init; }
        public string? Password { get; init; }
    }
}