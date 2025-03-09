using Newtonsoft.Json;
using System;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents a history record for a PRTG sensor.
    /// </summary>
    public class PrtgSensorHistoryItem
    {
        /// <summary>
        /// Gets or sets the original datetime value as provided by the JSON response.
        /// </summary>
        [JsonProperty("datetime")]
        public string DateTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parsed DateTime value.
        /// </summary>
        public DateTime DateTimeValue { get; set; }

        /// <summary>
        /// Gets or sets the string representation of the sensor value.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor status.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets any additional message associated with the sensor history record.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }
}