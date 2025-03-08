using Newtonsoft.Json;

namespace PRTGInsight.Models
{
    public class ConnectionInfo
    {
        [JsonProperty("serverUrl")]
        public string ServerUrl { get; set; } = string.Empty;

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonProperty("useApiKey")]
        public bool UseApiKey { get; set; }

        [JsonProperty("ignoreSslErrors")]
        public bool IgnoreSslErrors { get; set; } = true;

        [JsonProperty("prtgVersion")]
        public string PrtgVersion { get; set; } = string.Empty;

        // Override ToString for debugging
        public override string ToString()
        {
            return $"ServerUrl: {ServerUrl}, UseApiKey: {UseApiKey}, PrtgVersion: {PrtgVersion}";
        }
    }
}
