using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UniBot.Core.Options
{
    public class BotOptions
    {
        public string Endpoint { get; init; }
        public ExecutingOptions ExecutingOptions { get; init; } = new();
        public Dictionary<string, IConfigurationSection> MessengerOptions { get; init; }
    }
}