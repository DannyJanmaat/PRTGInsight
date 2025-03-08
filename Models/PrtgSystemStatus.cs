using System;

namespace PRTGInsight.Models
{
    public class PrtgSystemStatus
    {
        // Basic properties
        public string Version { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; } = DateTime.Now;

        // System metrics
        public string CpuLoadValue { get; set; } = string.Empty;
        public string MemoryUsageValue { get; set; } = string.Empty;
        public string UptimeValue { get; set; } = string.Empty;

        // Voor achterwaartse compatibiliteit
        public string CpuLoad
        {
            get => CpuLoadValue;
            set => CpuLoadValue = value;
        }

        public string MemoryUsage
        {
            get => MemoryUsageValue;
            set => MemoryUsageValue = value;
        }

        public string Uptime
        {
            get => UptimeValue;
            set => UptimeValue = value;
        }

        // Sensors
        public int TotalSensors { get; set; }
        public int UpSensors { get; set; }
        public int WarningSensors { get; set; }
        public int DownSensors { get; set; }
        public int PausedSensors { get; set; }
        public int UnusualSensors { get; set; }
        public int UndefinedSensors { get; set; }

        // Devices
        public int TotalDevices { get; set; }
        public int UpDevices { get; set; }
        public int WarningDevices { get; set; }
        public int DownDevices { get; set; }
        public int PausedDevices { get; set; }
        public int UnusualDevices { get; set; }
        public int UndefinedDevices { get; set; }

        // Groups
        public int TotalGroups { get; set; }
        public int UpGroups { get; set; }
        public int WarningGroups { get; set; }
        public int DownGroups { get; set; }
        public int PausedGroups { get; set; }
        public int UnusualGroups { get; set; }
        public int UndefinedGroups { get; set; }
    }
}
