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

        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsHuman { get; set; }
    }
}