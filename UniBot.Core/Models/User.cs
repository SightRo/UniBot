namespace UniBot.Core.Models
{
    public class User : InModelBase<long>
    {
        public User(long id, string messengerSource, bool isHuman)
            : base(id, messengerSource)
        {
            IsHuman = isHuman;
        }

        public string? Username { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public bool IsHuman { get; }
    }
}