namespace UniBot.Telegram
{
    public class Socks5
    {
        public string Hostname { get; set; } = null!;
        public int Port { get; set; } = 0;
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}