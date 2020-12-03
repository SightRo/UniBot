using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniBot.AspNetCore.Vkontakte
{
    [Serializable]
    public class Update
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("object")]
        public JObject Object { get; set; }
    }
}