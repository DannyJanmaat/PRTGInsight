using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace PRTGInsight.Models
{
    public partial class PrtgSensor
    {
        [JsonProperty("objid")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("lastvalue")]
        public string LastValue { get; set; } = string.Empty;

        [JsonProperty("device")]
        public string Device { get; set; } = string.Empty;

        [JsonProperty("deviceid")]
        public int DeviceId { get; set; }

        [JsonProperty("lastcheck")]
        public string LastCheck { get; set; } = string.Empty;

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("downtime")]
        public string Downtime { get; set; } = string.Empty;

        [JsonProperty("uptime")]
        public string Uptime { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("interval")]
        public string Interval { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("access")]
        public string Access { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [GeneratedRegex("<.*?>")]
        private static partial Regex HtmlTagRegex();

        public DateTime LastCheckDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(LastCheck))
                    return DateTime.MinValue;

                try
                {
                    // Probeer eerst standaard parsing
                    if (DateTime.TryParse(LastCheck, out DateTime result))
                        return result;

                    // Als dat niet lukt, probeer het formaat te herkennen en te parsen
                    // Verwijder HTML-tags zoals <span class="percent">[29 s ago]</span>
                    string cleanedString = HtmlTagRegex().Replace(LastCheck, string.Empty);

                    // Als het een relatieve tijd is (bijv. "29 s ago"), retourneer de huidige tijd minus die tijd
                    if (cleanedString.Contains("ago"))
                    {
                        // Eenvoudige benadering: retourneer gewoon de huidige tijd
                        return DateTime.Now;
                    }

                    // Probeer verschillende datumformaten
                    string[] formats =
                    [
                "yyyy-MM-dd-HH-mm-ss",
                    "yyyy-MM-dd HH:mm:ss",
                    "dd-MM-yyyy HH:mm:ss",
                    "MM/dd/yyyy HH:mm:ss"
            ];

                    if (DateTime.TryParseExact(cleanedString, formats, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        return parsedDate;
                    }

                    // Als alles faalt, retourneer de huidige tijd
                    System.Diagnostics.Debug.WriteLine($"Could not parse date: {LastCheck}");
                    return DateTime.Now;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing date: {ex.Message}");
                    return DateTime.Now;
                }
            }
        }
    }
}
