namespace UniBot.Core.Models
{
    public class User : InModelBase<long>
    {
        public string Username { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsHuman { get; set; }
    }
}