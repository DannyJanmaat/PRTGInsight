using Newtonsoft.Json;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents a device within the PRTG Network Monitor system.
    /// </summary>
    public class PrtgDevice
    {
        /// <summary>
        /// Gets or sets the unique device identifier.
        /// </summary>
        [JsonProperty("objid")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the device.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current status of the device.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message associated with the device.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the device is active.
        /// </summary>
        [JsonProperty("active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the device.
        /// </summary>
        [JsonProperty("tags")]
        public string Tags { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the priority of the device.
        /// </summary>
        [JsonProperty("priority")]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the host name of the device.
        /// </summary>
        [JsonProperty("host")]
        public string HostName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent device identifier.
        /// </summary>
        [JsonProperty("parentid")]
        public int ParentId { get; set; }
    }
}