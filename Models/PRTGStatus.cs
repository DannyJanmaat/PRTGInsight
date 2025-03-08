namespace PRTGInsight.Models
{
    public class PrtgStatus
    {
        public bool IsConnected { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
