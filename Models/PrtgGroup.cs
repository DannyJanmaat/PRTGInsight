using Newtonsoft.Json;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents a group within PRTG, containing sensor summary statistics.
    /// </summary>
    public class PrtgGroup
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group.
        /// </summary>
        [JsonProperty("objid")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current status of the group.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of sensors in the group.
        /// </summary>
        [JsonProperty("totalsens")]
        public int TotalSensors { get; set; }

        /// <summary>
        /// Gets or sets the count of sensors that are currently down.
        /// </summary>
        [JsonProperty("downcount")]
        public int DownCount { get; set; }

        /// <summary>
        /// Gets or sets the count of sensors that are down but have been acknowledged.
        /// </summary>
        [JsonProperty("downacksens")]
        public int DownAcknowledgedSensors { get; set; }

        /// <summary>
        /// Gets or sets the count of sensors that are currently up.
        /// </summary>
        [JsonProperty("upsens")]
        public int UpSensors { get; set; }

        /// <summary>
        /// Gets or sets the count of sensors with a warning status.
        /// </summary>
        [JsonProperty("warnsens")]
        public int WarningSensors { get; set; }

        /// <summary>
        /// Gets or sets the count of sensors that are paused.
        /// </summary>
        [JsonProperty("pausedsens")]
        public int PausedSensors { get; set; }
    }
}