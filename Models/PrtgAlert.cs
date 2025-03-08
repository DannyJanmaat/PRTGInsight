using Newtonsoft.Json;

namespace PRTGInsight.Models
{
    public class PrtgAlert
    {
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("datetime")]
        public string TimeStamp { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("priority")]
        public string Priority { get; set; } = string.Empty;
    }
}
