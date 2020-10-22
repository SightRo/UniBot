using UniBot.Core.Settings;

namespace UniBot.Vkontakte
{
    public class VkOptions : MessengerOptions
    {
        public ulong GroupId { get; set; } = 0;
        public string Confirmation { get; set; }
    }
}