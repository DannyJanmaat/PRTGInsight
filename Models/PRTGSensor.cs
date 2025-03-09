using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents a sensor object from PRTG.
    /// </summary>
    public partial class PrtgSensor
    {
        /// <summary>
        /// Gets or sets the sensor identifier.
        /// </summary>
        [JsonProperty("objid")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the sensor name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor status.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets any associated message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last reported value.
        /// </summary>
        [JsonProperty("lastvalue")]
        public string LastValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device name to which the sensor belongs.
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        [JsonProperty("deviceid")]
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the last check timestamp as received from the server.
        /// </summary>
        [JsonProperty("lastcheck")]
        public string LastCheck { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the sensor is active.
        /// </summary>
        [JsonProperty("active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the downtime message.
        /// </summary>
        [JsonProperty("downtime")]
        public string Downtime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the uptime message.
        /// </summary>
        [JsonProperty("uptime")]
        public string Uptime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor interval.
        /// </summary>
        [JsonProperty("interval")]
        public string Interval { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor priority.
        /// </summary>
        [JsonProperty("priority")]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the access rights for the sensor.
        /// </summary>
        [JsonProperty("access")]
        public string Access { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tags associated with the sensor.
        /// </summary>
        [JsonProperty("tags")]
        public string Tags { get; set; } = string.Empty;

        /// <summary>
        /// Gets the regular expression used to remove HTML tags.
        /// </summary>
        [GeneratedRegex("<.*?>")]
        private static partial Regex HtmlTagRegex();

        /// <summary>
        /// Gets the parsed <see cref="DateTime"/> value from the <see cref="LastCheck"/> string.
        /// Returns <see cref="DateTime.MinValue"/> if the <see cref="LastCheck"/> is empty.
        /// If parsing fails, returns the current time.
        /// </summary>
        public DateTime LastCheckDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(LastCheck))
                {
                    return DateTime.MinValue;
                }

                try
                {
                    // Attempt standard date parsing.
                    if (DateTime.TryParse(LastCheck, out DateTime result))
                    {
                        return result;
                    }

                    // Remove HTML tags (e.g. <span class="percent">[29 s ago]</span>) if present.
                    string cleanedString = HtmlTagRegex().Replace(LastCheck, string.Empty);

                    // If the cleaned string indicates relative time (e.g., "29 s ago"), return current time.
                    if (cleanedString.Contains("ago", StringComparison.OrdinalIgnoreCase))
                    {
                        return DateTime.Now;
                    }

                    // Try to parse using an array of known date formats.
                    var formats = new[]
                    {
                        "yyyy-MM-dd-HH-mm-ss",
                        "yyyy-MM-dd HH:mm:ss",
                        "dd-MM-yyyy HH:mm:ss",
                        "MM/dd/yyyy HH:mm:ss"
                    };

                    if (DateTime.TryParseExact(cleanedString, formats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime parsedDate))
                    {
                        return parsedDate;
                    }

                    Debug.WriteLine($"Could not parse date: {LastCheck}");
                    return DateTime.Now;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing LastCheck date: {ex.Message}");
                    return DateTime.Now;
                }
            }
        }
    }
}