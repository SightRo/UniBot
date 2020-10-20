using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UniBot.Core.Settings
{
    public class BotOptions
    {
        public string Endpoint { get; set; }
        public Dictionary<string, IConfigurationSection> MessengerOptions { get; set; }
    }
}