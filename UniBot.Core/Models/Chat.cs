namespace UniBot.Core.Models
{
    public class Chat : InModelBase<long>
    {
        public string Name { get; set; } = null!;
        public long Owner { get; set; }
        public bool IsUserConversation { get; set; }
        public bool IsGroupConversation => !IsUserConversation;
    }
}