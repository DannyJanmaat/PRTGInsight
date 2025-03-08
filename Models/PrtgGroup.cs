using Newtonsoft.Json;

namespace PRTGInsight.Models
{
    public class PrtgGroup
    {
        [JsonProperty("objid")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("totalsens")]
        public int TotalSensors { get; set; }

        [JsonProperty("downcount")]
        public int DownCount { get; set; }

        [JsonProperty("downacksens")]
        public int DownAcknowledgedSensors { get; set; }

        [JsonProperty("upsens")]
        public int UpSensors { get; set; }

        [JsonProperty("warnsens")]
        public int WarningSensors { get; set; }

        [JsonProperty("pausedsens")]
        public int PausedSensors { get; set; }
    }
}
