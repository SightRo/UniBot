using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UniBot.Core.Settings
{
    public class BotOptions
    {
        public string Endpoint { get; set; }
        public ExecutingOptions ExecutingOptions { get; set; } = new ExecutingOptions();
        public Dictionary<string, IConfigurationSection> MessengerOptions { get; set; }
    }
}