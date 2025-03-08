namespace PRTGInsight.Models
{
    public class AppSettings
    {
        // Connection settings
        public ConnectionInfo LastConnection { get; set; } = new ConnectionInfo();

        // App theme settings
        public string Theme { get; set; } = "System";  // "Light", "Dark", or "System"

        // UI preferences
        public bool EnableAnimations { get; set; } = true;
        public bool ShowStatusIcons { get; set; } = true;
        public bool ConfirmBeforeClosing { get; set; } = true;

        // Data refresh settings
        public int AutoRefreshInterval { get; set; } = 300;  // in seconds, 0 = disabled
        public bool EnableAutoRefresh { get; set; } = true;

        // Alerts settings
        public bool EnableNotifications { get; set; } = true;
        public bool SoundAlerts { get; set; } = false;

        // Log settings
        public bool EnableDebugLogging { get; set; } = false;
        public int MaxLogSize { get; set; } = 10;  // in MB
    }
}