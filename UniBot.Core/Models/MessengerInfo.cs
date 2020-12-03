using UniBot.Core.Options;

namespace UniBot.Core.Models
{
    public class MessengerInfo
    {
        public MessengerInfo(string name, object api, MessengerOptions options)
        {
            Name = name;
            Api = api;
            Options = options;
        }

        public string Name { get; init; }
        public object Api { get; init; }
        public MessengerOptions Options { get; init; }
        // public SupportedApi SupportedApi { get; init; }
    }
}