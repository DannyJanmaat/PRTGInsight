using Newtonsoft.Json;

namespace PRTGInsight.Models
{
    public class PrtgDevice
    {
        [JsonProperty("objid")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; } = string.Empty;

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("host")]
        public string HostName { get; set; } = string.Empty;

        [JsonProperty("parentid")]
        public int ParentId { get; set; }
    }
}
