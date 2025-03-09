using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using PRTGInsight.Services.Prtg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;

namespace PRTGInsight.Views
{
    public sealed partial class AlertsPage : Page
    {
        private ConnectionInfo _connectionInfo;
        private List<PrtgAlert> _alerts;
        private CancellationTokenSource _cts;

        public AlertsPage()
        {
            this.InitializeComponent();
            _alerts = [];

            this.Loaded += AlertsPage_Loaded;
        }

        private void AlertsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAlertsData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ConnectionInfo connectionInfo)
            {
                _connectionInfo = connectionInfo;
            }
            else
            {
                // Try to load connection info if not passed
                _connectionInfo = ConnectionManager.CurrentConnection;

                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    ShowConnectionError();
                }
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

            // Navigate to connection page
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                _ = Frame.Navigate(typeof(ConnectionPage));
            }
        }

        public async void RefreshData()
        {
            await LoadAlertsAsync();
        }

        private async void LoadAlertsData()
        {
            if (_connectionInfo != null && !string.IsNullOrEmpty(_connectionInfo.ServerUrl))
            {
                await LoadAlertsAsync();
            }
        }

        private async Task LoadAlertsAsync()
        {
            try
            {
                // Cancel any ongoing operations
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                // Show loading indicator
                if (LoadingIndicator != null)
                {
                    LoadingIndicator.IsActive = true;
                }

                // Get alerts from service using PrtgDashboardService as the replacement for PrtgAlertService
                _alerts = await PrtgDashboardService.GetRecentAlertsAsync(_connectionInfo, _cts.Token);

                // Convert to view models
                List<AlertViewModel> alertViewModels = [.. _alerts.Select(a => new AlertViewModel(a))];

                // Update UI
                if (AlertsListView != null)
                {
                    AlertsListView.ItemsSource = alertViewModels;
                }

                // Show "No Data" message if no alerts
                if (NoDataMessage != null)
                {
                    NoDataMessage.Visibility = (alertViewModels.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading alerts: {ex.Message}");

                // Show error message
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to load alerts: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await dialog.ShowAsync();
            }
            finally
            {
                // Hide loading indicator
                if (LoadingIndicator != null)
                {
                    LoadingIndicator.IsActive = false;
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAlertsAsync();
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            // This would show a filter dialog in a real implementation
            ContentDialog dialog = new()
            {
                Title = "Filter Alerts",
                Content = "Alert filtering is not implemented yet.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = await dialog.ShowAsync();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all filters
            if (AlertsListView != null)
            {
                AlertsListView.ItemsSource = _alerts.Select(a => new AlertViewModel(a)).ToList();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Cancel any ongoing operations
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    public class AlertViewModel(PrtgAlert alert)
    {
        public string Message { get; } = alert.Message ?? "No message";
        public string TimeStamp { get; } = alert.TimeStamp ?? "Unknown time";
        public string Status { get; } = alert.Status ?? "Unknown";
        public string Priority { get; } = alert.Priority ?? "Normal";
        public SolidColorBrush StatusColorBrush { get; } = new SolidColorBrush(GetStatusColor(alert.Status));

        private static Color GetStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return Color.FromArgb(255, 128, 128, 128); // Gray for unknown
            }

            return status.ToLower() switch
            {
                "up" => Color.FromArgb(255, 43, 123, 43),      // Green
                "warning" => Color.FromArgb(255, 228, 155, 15), // Orange
                "down" => Color.FromArgb(255, 209, 52, 56),     // Red
                _ => Color.FromArgb(255, 128, 128, 128)         // Gray
            };
        }
    }
}
