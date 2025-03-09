using Newtonsoft.Json;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents the connection configuration details for a PRTG server.
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// Gets or sets the base URL of the PRTG server.
        /// </summary>
        [JsonProperty("serverUrl")]
        public string ServerUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username for authentication.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for authentication.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the API key for authentication.
        /// </summary>
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to use API key authentication.
        /// </summary>
        [JsonProperty("useApiKey")]
        public bool UseApiKey { get; set; }

        // Backing field to force ignoring SSL errors (always true).
        private readonly bool _alwaysIgnoreSslErrors = true;

        /// <summary>
        /// Gets or sets a value indicating whether SSL errors should be ignored.
        /// This property is always true.
        /// </summary>
        [JsonProperty("ignoreSslErrors")]
        public bool IgnoreSslErrors
        {
            get => _alwaysIgnoreSslErrors;
            set { /* Ignored: Always returns true for SSL errors */ }
        }

        /// <summary>
        /// Gets or sets the version of the connected PRTG server.
        /// </summary>
        [JsonProperty("prtgVersion")]
        public string PrtgVersion { get; set; } = string.Empty;

        /// <summary>
        /// Returns a string representation of the current connection information.
        /// </summary>
        /// <returns>A string with server URL, API usage flag, and PRTG version.</returns>
        public override string ToString()
        {
            return $"ServerUrl: {ServerUrl}, UseApiKey: {UseApiKey}, PrtgVersion: {PrtgVersion}";
        }
    }
}
