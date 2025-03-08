using PRTGInsight.Models;

namespace PRTGInsight.Services
{
    /// <summary>
    /// Extension methods for working with PRTG device statuses
    /// </summary>
    public static class StatusColorExtensions
    {
        public static Windows.UI.Color GetStatusColor(this PrtgDevice device)
        {
            if (device == null) return Windows.UI.Color.FromArgb(255, 128, 128, 128);

            return device.Status.ToLowerInvariant() switch
            {
                "up" => Windows.UI.Color.FromArgb(255, 43, 123, 43),      // Green
                "warning" => Windows.UI.Color.FromArgb(255, 228, 155, 15), // Orange
                "down" => Windows.UI.Color.FromArgb(255, 209, 52, 56),     // Red
                "paused" => Windows.UI.Color.FromArgb(255, 128, 128, 128), // Gray
                _ => Windows.UI.Color.FromArgb(255, 128, 128, 128)         // Gray
            };
        }
    }
}
