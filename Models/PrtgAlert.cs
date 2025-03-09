using Newtonsoft.Json;
using System;
using System.Globalization;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents an alert or notification from the PRTG server.
    /// </summary>
    public class PrtgAlert
    {
        /// <summary>
        /// Gets or sets the alert message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the alert timestamp as a string.
        /// </summary>
        [JsonProperty("datetime")]
        public string TimeStamp { get; set; } = string.Empty;

        /// <summary>
        /// Gets the parsed <see cref="DateTime"/> from the <see cref="TimeStamp"/> string.
        /// Returns <see cref="DateTime.MinValue"/> if parsing fails.
        /// </summary>
        public DateTime TimeStampParsed =>
            DateTime.TryParse(TimeStamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
                ? result : DateTime.MinValue;

        /// <summary>
        /// Gets or sets the alert status.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the priority of the alert.
        /// </summary>
        [JsonProperty("priority")]
        public string Priority { get; set; } = string.Empty;
    }
}
