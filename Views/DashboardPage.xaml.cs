using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PRTGInsight.Views
{
    public sealed partial class DashboardPage : Page
    {
        private ConnectionInfo _connectionInfo;
        private CancellationTokenSource _cts;

        public DashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ConnectionInfo connectionInfo)
            {
                _connectionInfo = connectionInfo;
                LoadData();
            }
            else
            {
                // Try to load connection info from settings
                _connectionInfo = ConnectionManager.CurrentConnection;

                if (_connectionInfo != null)
                {
                    LoadData();
                }
                else
                {
                    ShowConnectionError();
                }
            }

            // Ensure the navigation pane is open when dashboard is loaded
            EnsureNavigationPaneIsOpen();
        }

        private void EnsureNavigationPaneIsOpen()
        {
            try
            {
                // Get the current window and access its MainNavigationView
                if (App.MainWindow?.Content is Grid mainGrid &&
                    mainGrid.FindName("MainNavigationView") is NavigationView navigationView)
                {
                    navigationView.IsPaneOpen = true;
                    navigationView.Visibility = Visibility.Visible;
                    Debug.WriteLine("Navigation pane opened on dashboard load");
                }
                else
                {
                    Debug.WriteLine("MainNavigationView not found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening navigation pane: {ex.Message}");
            }
        }

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the current window and access its MainNavigationView
                if (App.MainWindow?.Content is Grid mainGrid &&
                    mainGrid.FindName("MainNavigationView") is NavigationView navigationView)
                {
                    // Ensure it's visible
                    navigationView.Visibility = Visibility.Visible;

                    // Force layout update
                    navigationView.UpdateLayout();

                    // Toggle the pane
                    navigationView.IsPaneOpen = !navigationView.IsPaneOpen;

                    Debug.WriteLine($"Toggled MainNavigationView.IsPaneOpen to {navigationView.IsPaneOpen}");
                }
                else
                {
                    Debug.WriteLine("MainNavigationView not found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling menu: {ex.Message}");
            }
        }

        private async void LoadData()
        {
            try
            {
                // Update connection info
                if (_connectionInfo != null)
                {
                    ServerUrlTextBlock.Text = $"Connected to: {_connectionInfo.ServerUrl}";
                    PrtgVersionTextBlock.Text = $"PRTG Version: {_connectionInfo.PrtgVersion ?? "Unknown"}";
                }

                // Load dashboard data
                await LoadDashboardData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        public async void RefreshData()
        {
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                // Create dummy data for now
                SensorCountTextBlock.Text = "123";
                DeviceCountTextBlock.Text = "45";
                UpSensorsTextBlock.Text = "100";
                DownSensorsTextBlock.Text = "5";
                WarningSensorsTextBlock.Text = "10";
                PausedSensorsTextBlock.Text = "8";

                CpuLoadTextBlock.Text = "23%";
                MemoryUsageTextBlock.Text = "45%";
                UptimeTextBlock.Text = "10 days, 4 hours";

                // Dummy alerts
                List<AlertItem> alerts =
                [
                    new AlertItem { Message = "Sensor Down: Server01 - CPU Load", Priority = "High", TimeStamp = "Today 10:45", StatusColor = "#F44336" },
                    new AlertItem { Message = "Warning: Server02 - Memory Usage above 80%", Priority = "Medium", TimeStamp = "Today 09:30", StatusColor = "#FF9800" }
                ];

                AlertsListView.ItemsSource = alerts;
                AlertsEmptyState.Visibility = alerts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

                // Dummy top sensors
                List<SensorItem> sensors =
                [
                    new SensorItem { Name = "Server01 - CPU Load", Device = "Server01", LastValue = "85%", Status = "Warning", StatusColor = "#FF9800" },
                    new SensorItem { Name = "Server02 - Memory Usage", Device = "Server02", LastValue = "92%", Status = "Warning", StatusColor = "#FF9800" },
                    new SensorItem { Name = "Server03 - Disk Space", Device = "Server03", LastValue = "15% free", Status = "Down", StatusColor = "#F44336" }
                ];

                TopSensorsListView.ItemsSource = sensors;
                SensorsEmptyState.Visibility = sensors.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        private async void ShowConnectionError()
        {
            ContentDialog dialog = new()
            {
                Title = "Connection Error",
                Content = "No connection information available. Please connect to a PRTG server.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            _ = await dialog.ShowAsync();

            _ = Frame.Navigate(typeof(ConnectionPage));
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDashboardData();
        }

        private async void ExportDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialog dialog = new()
                {
                    Title = "Export Dashboard",
                    Content = "This feature is not yet implemented.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                _ = await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting dashboard: {ex.Message}");
            }
        }

        private async void ViewAllAlertsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = Frame.Navigate(typeof(AlertsPage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to alerts: {ex.Message}");

                ContentDialog dialog = new()
                {
                    Title = "View All Alerts",
                    Content = "Navigation to Alerts page failed.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                _ = await dialog.ShowAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    public class AlertItem
    {
        public string Message { get; set; }
        public string Priority { get; set; }
        public string TimeStamp { get; set; }
        public string StatusColor { get; set; }
    }

    public class SensorItem
    {
        public string Name { get; set; }
        public string Device { get; set; }
        public string LastValue { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
    }
}