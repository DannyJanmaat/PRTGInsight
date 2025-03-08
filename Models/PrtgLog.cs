using Newtonsoft.Json;
using System;

namespace PRTGInsight.Models
{
    public class PrtgLog
    {
        [JsonProperty("objid")]
        public int Id { get; set; }

        [JsonProperty("datetime")]
        public string DateTime { get; set; } = string.Empty;

        [JsonProperty("parent")]
        public string Parent { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        public DateTime DateTimeValue => string.IsNullOrEmpty(DateTime) ?
            System.DateTime.MinValue : System.DateTime.Parse(DateTime);
    }
}
