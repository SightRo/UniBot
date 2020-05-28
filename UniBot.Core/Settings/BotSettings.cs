using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UniBot.Core.Settings
{
    public class BotSettings
    {
        public string Endpoint { get; set; }
        public Dictionary<string, IConfigurationSection> Messengers { get; set; }
    }
}