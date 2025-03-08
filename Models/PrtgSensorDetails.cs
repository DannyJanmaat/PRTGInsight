using System;

namespace PRTGInsight.Models
{
    public class PrtgSensorDetails
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string LastValue { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public int DeviceId { get; set; }
        public string LastCheck { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Downtime { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Interval { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string AccessRights { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;

        public DateTime LastCheckDateTime => string.IsNullOrEmpty(LastCheck) ?
            DateTime.MinValue : DateTime.Parse(LastCheck);
    }
}
