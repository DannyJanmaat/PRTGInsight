using System;

namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents overall system status information retrieved from a PRTG server.
    /// </summary>
    public class PrtgSystemStatus
    {
        #region Basic Properties

        /// <summary>
        /// Gets or sets the version of the PRTG server.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the server time as reported by the PRTG server.
        /// </summary>
        public DateTime ServerTime { get; set; } = DateTime.Now;

        #endregion

        #region System Metrics

        /// <summary>
        /// Gets or sets the CPU load as a string.
        /// </summary>
        public string CpuLoadValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the memory usage as a string.
        /// </summary>
        public string MemoryUsageValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the uptime as a string.
        /// </summary>
        public string UptimeValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CPU load. (For backward compatibility)
        /// </summary>
        public string CpuLoad
        {
            get => CpuLoadValue;
            set => CpuLoadValue = value;
        }

        /// <summary>
        /// Gets or sets the memory usage. (For backward compatibility)
        /// </summary>
        public string MemoryUsage
        {
            get => MemoryUsageValue;
            set => MemoryUsageValue = value;
        }

        /// <summary>
        /// Gets or sets the uptime. (For backward compatibility)
        /// </summary>
        public string Uptime
        {
            get => UptimeValue;
            set => UptimeValue = value;
        }

        #endregion

        #region Sensor Metrics

        /// <summary>
        /// Gets or sets the total number of sensors.
        /// </summary>
        public int TotalSensors { get; set; }

        /// <summary>
        /// Gets or sets the number of sensors that are up.
        /// </summary>
        public int UpSensors { get; set; }

        /// <summary>
        /// Gets or sets the number of sensors with a warning status.
        /// </summary>
        public int WarningSensors { get; set; }

        /// <summary>
        /// Gets or sets the number of sensors that are down.
        /// </summary>
        public int DownSensors { get; set; }

        /// <summary>
        /// Gets or sets the number of sensors that are paused.
        /// </summary>
        public int PausedSensors { get; set; }

        /// <summary>
        /// Gets or sets the number of sensors with unusual status.
        /// </summary>
        public int UnusualSensors { get; set; }

        /// <summary>
        /// Gets or sets the number of sensors with an undefined status.
        /// </summary>
        public int UndefinedSensors { get; set; }

        #endregion

        #region Device Metrics

        /// <summary>
        /// Gets or sets the total number of devices.
        /// </summary>
        public int TotalDevices { get; set; }

        /// <summary>
        /// Gets or sets the number of devices that are up.
        /// </summary>
        public int UpDevices { get; set; }

        /// <summary>
        /// Gets or sets the number of devices with a warning status.
        /// </summary>
        public int WarningDevices { get; set; }

        /// <summary>
        /// Gets or sets the number of devices that are down.
        /// </summary>
        public int DownDevices { get; set; }

        /// <summary>
        /// Gets or sets the number of devices that are paused.
        /// </summary>
        public int PausedDevices { get; set; }

        /// <summary>
        /// Gets or sets the number of devices with unusual status.
        /// </summary>
        public int UnusualDevices { get; set; }

        /// <summary>
        /// Gets or sets the number of devices with an undefined status.
        /// </summary>
        public int UndefinedDevices { get; set; }

        #endregion

        #region Group Metrics

        /// <summary>
        /// Gets or sets the total number of groups.
        /// </summary>
        public int TotalGroups { get; set; }

        /// <summary>
        /// Gets or sets the number of groups that are up.
        /// </summary>
        public int UpGroups { get; set; }

        /// <summary>
        /// Gets or sets the number of groups with a warning status.
        /// </summary>
        public int WarningGroups { get; set; }

        /// <summary>
        /// Gets or sets the number of groups that are down.
        /// </summary>
        public int DownGroups { get; set; }

        /// <summary>
        /// Gets or sets the number of groups that are paused.
        /// </summary>
        public int PausedGroups { get; set; }

        /// <summary>
        /// Gets or sets the number of groups with unusual status.
        /// </summary>
        public int UnusualGroups { get; set; }

        /// <summary>
        /// Gets or sets the number of groups with an undefined status.
        /// </summary>
        public int UndefinedGroups { get; set; }

        #endregion
    }
}