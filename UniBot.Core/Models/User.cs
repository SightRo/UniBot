namespace UniBot.Core.Models
{
    public class User : InModelBase<long>
    {
        public User(
            long id,
            string messengerSource,
            string? username,
            string? firstName,
            string? lastName,
            bool isHuman)
            : base(id, messengerSource)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            IsHuman = isHuman;
        }

        public string? Username { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public bool IsHuman { get; init; }
    }
}