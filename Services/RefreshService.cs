using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PRTGInsight.Views;
using System;
using System.Diagnostics;

namespace PRTGInsight.Services
{
    public static class RefreshService
    {
        private static DispatcherTimer _refreshTimer;

        public static void StartAutoRefresh(int intervalSeconds)
        {
            StopAutoRefresh(); // Stop any existing timer

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(intervalSeconds)
            };

            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            Debug.WriteLine($"Auto refresh started with interval of {intervalSeconds} seconds");
        }

        public static void StopAutoRefresh()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Tick -= RefreshTimer_Tick;
                _refreshTimer = null;

                Debug.WriteLine("Auto refresh stopped");
            }
        }

        public static void UpdateRefreshInterval(int intervalSeconds)
        {
            if (_refreshTimer != null && _refreshTimer.IsEnabled)
            {
                _refreshTimer.Interval = TimeSpan.FromSeconds(intervalSeconds);
                Debug.WriteLine($"Auto refresh interval updated to {intervalSeconds} seconds");
            }
        }

        private static void RefreshTimer_Tick(object sender, object e)
        {
            try
            {
                // Refresh the current page based on its type
                if (App.MainWindow?.Content is Frame mainFrame && mainFrame.Content is Frame contentFrame)
                {
                    var currentPage = contentFrame.Content;

                    if (currentPage is DashboardPage dashboardPage)
                    {
                        // Refresh dashboard
                        dashboardPage.RefreshData();
                    }
                    else if (currentPage is SensorsPage sensorsPage)
                    {
                        // Refresh sensors data
                        sensorsPage.RefreshData();
                    }
                    else if (currentPage is DevicesPage devicesPage)
                    {
                        // Refresh devices data
                        devicesPage.RefreshData();
                    }
                    else if (currentPage is AlertsPage alertsPage)
                    {
                        // Refresh alerts data
                        alertsPage.RefreshData();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in auto refresh: {ex.Message}");
            }
        }
    }
}
