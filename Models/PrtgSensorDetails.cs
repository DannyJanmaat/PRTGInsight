using System;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents detailed information for a PRTG sensor.
    /// </summary>
    public class PrtgSensorDetails
    {
        /// <summary>
        /// Gets or sets the sensor identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the sensor name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets any associated message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last reported value.
        /// </summary>
        public string LastValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device name to which the sensor belongs.
        /// </summary>
        public string Device { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the last check timestamp as a string.
        /// </summary>
        public string LastCheck { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the sensor is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the downtime message.
        /// </summary>
        public string Downtime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the uptime message.
        /// </summary>
        public string Uptime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor interval.
        /// </summary>
        public string Interval { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sensor priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the access rights of the sensor.
        /// </summary>
        public string AccessRights { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets tags associated with the sensor.
        /// </summary>
        public string Tags { get; set; } = string.Empty;

        /// <summary>
        /// Gets the parsed <see cref="DateTime"/> from the <see cref="LastCheck"/> string.
        /// Returns <see cref="DateTime.MinValue"/> if parsing fails.
        /// </summary>
        public DateTime LastCheckDateTime
        {
            get
            {
                return DateTime.TryParse(LastCheck, out DateTime result) ? result : DateTime.MinValue;
            }
        }
    }
}
