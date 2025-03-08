using Newtonsoft.Json;
using System;

namespace PRTGInsight.Models
{
    public class PrtgSensorHistoryItem
    {
        [JsonProperty("datetime")]
        public string DateTime { get; set; } = string.Empty;

        // Make this property have a setter
        public DateTime DateTimeValue { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }
}
