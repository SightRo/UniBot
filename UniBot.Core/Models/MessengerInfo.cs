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

        public string Name { get; }
        public object Api { get; }
        public MessengerOptions Options { get; }
        // public SupportedApi SupportedApi { get; init; }
    }
}