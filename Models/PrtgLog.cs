using Newtonsoft.Json;
using System;
using System.Globalization;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents a log entry from the PRTG server.
    /// </summary>
    public class PrtgLog
    {
        /// <summary>
        /// Gets or sets the log identifier.
        /// </summary>
        [JsonProperty("objid")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time as a string from the log.
        /// </summary>
        [JsonProperty("datetime")]
        public string DateTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent associated with the log entry.
        /// </summary>
        [JsonProperty("parent")]
        public string Parent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the log entry.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name associated with the log entry.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the log entry.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed message from the log entry.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets the parsed <see cref="DateTime"/> based on the <see cref="DateTime"/> string.
        /// Returns <see cref="System.DateTime.MinValue"/> if the string is empty or cannot be parsed.
        /// </summary>
        public DateTime DateTimeValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DateTime))
                    return System.DateTime.MinValue;

                return System.DateTime.TryParse(DateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                    ? dt
                    : System.DateTime.MinValue;
            }
        }
    }
}
