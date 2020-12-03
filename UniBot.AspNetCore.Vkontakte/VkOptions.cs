using UniBot.Core.Options;

namespace UniBot.AspNetCore.Vkontakte
{
    public class VkOptions : MessengerOptions
    {
        public ulong GroupId { get; init; } = 0;
    }
}