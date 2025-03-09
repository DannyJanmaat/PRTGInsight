namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents the application settings for PRTGInsight.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the last connection information used for connecting to a PRTG server.
        /// </summary>
        public ConnectionInfo LastConnection { get; set; } = new ConnectionInfo();

        /// <summary>
        /// Gets or sets the application theme.
        /// Possible values: "Light", "Dark", or "System".
        /// </summary>
        public string Theme { get; set; } = "System";

        #region UI Preferences

        /// <summary>
        /// Gets or sets a value indicating whether animations are enabled in the UI.
        /// </summary>
        public bool EnableAnimations { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether status icons are shown in the UI.
        /// </summary>
        public bool ShowStatusIcons { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether a confirmation dialog is shown before closing the application.
        /// </summary>
        public bool ConfirmBeforeClosing { get; set; } = true;

        #endregion

        #region Data Refresh Settings

        /// <summary>
        /// Gets or sets the auto-refresh interval in seconds.
        /// A value of 0 disables auto-refresh.
        /// </summary>
        public int AutoRefreshInterval { get; set; } = 300;

        /// <summary>
        /// Gets or sets a value indicating whether auto-refresh is enabled.
        /// </summary>
        public bool EnableAutoRefresh { get; set; } = true;

        #endregion

        #region Alerts Settings

        /// <summary>
        /// Gets or sets a value indicating whether notifications are enabled.
        /// </summary>
        public bool EnableNotifications { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether sound alerts are enabled.
        /// </summary>
        public bool SoundAlerts { get; set; } = false;

        #endregion

        #region Log Settings

        /// <summary>
        /// Gets or sets a value indicating whether debug logging is enabled.
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum log size in megabytes.
        /// </summary>
        public int MaxLogSize { get; set; } = 10;

        #endregion
    }
}
