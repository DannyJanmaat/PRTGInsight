// PrtgSensorStatus.cs
namespace PRTGInsight.Models
{
    public class PrtgSensorStatus
    {
        public int TotalSensors { get; set; }
        public int UpSensors { get; set; }
        public int DownSensors { get; set; }
        public int TotalDevices { get; set; }
    }
}