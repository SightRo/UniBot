using UniBot.Core.Settings;

namespace UniBot.Vkontakte
{
    public class VkSettings : MessengerOptions
    {
        public ulong GroupId { get; set; } = 0;
        public string Confirmation { get; set; }
    }
}