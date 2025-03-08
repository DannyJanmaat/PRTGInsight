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
    public sealed partial class DashboardPage : Page
    {
        private readonly PrtgService _prtgService;
        private ConnectionInfo _connectionInfo;
        private CancellationTokenSource _cancellationTokenSource;

        public DashboardPage()
        {
            this.InitializeComponent();
            _prtgService = new PrtgService();
            _connectionInfo = new ConnectionInfo();

            this.Loaded += OnPageLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Debug.WriteLine("DashboardPage: OnNavigatedTo called");

            if (e.Parameter is ConnectionInfo connectionInfo)
            {
                Debug.WriteLine("DashboardPage: Connection info received via navigation parameter");
                _connectionInfo = connectionInfo;
            }
            else
            {
                Debug.WriteLine("DashboardPage: No connection info in navigation parameter, getting from ConnectionManager");
                _connectionInfo = ConnectionManager.CurrentConnection;
            }

            // Debug connection info
            if (_connectionInfo != null)
            {
                Debug.WriteLine($"DashboardPage: Connection info - URL={_connectionInfo.ServerUrl}, UseApiKey={_connectionInfo.UseApiKey}");
            }
            else
            {
                Debug.WriteLine("DashboardPage: Connection info is null");
            }
        }

        private async void OnPageLoaded(object _, RoutedEventArgs __)
        {
            Debug.WriteLine("DashboardPage: OnPageLoaded called");

            // Ensure we have connection info
            if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
            {
                Debug.WriteLine("DashboardPage: Connection info is null or empty, getting from ConnectionManager");
                _connectionInfo = ConnectionManager.CurrentConnection;

                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    Debug.WriteLine("DashboardPage: Still no connection info, showing error");
                    ShowError("No connection information available. Please connect to a PRTG server.");
                    return;
                }
            }

            // Update UI with connection info
            ServerUrlTextBlock.Text = $"Connected to: {_connectionInfo.ServerUrl}";
            PrtgVersionTextBlock.Text = $"PRTG Version: {_connectionInfo.PrtgVersion}";

            // Load dashboard data
            await LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                Debug.WriteLine("DashboardPage: LoadDashboardDataAsync called");

                // Show loading indicators
                LoadingRing.IsActive = true;
                NoAlertsTextBlock.Visibility = Visibility.Collapsed;
                ErrorInfoBar.IsOpen = false;

                // Cancel any ongoing operations
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                // Load dashboard data in parallel
                var tasks = new List<Task>
        {
            LoadSensorStatusAsync(token),
            LoadSystemStatusAsync(token),
            LoadRecentAlertsAsync(token)
        };

                // Wait for all tasks to complete
                await Task.WhenAll(tasks);

                Debug.WriteLine("DashboardPage: All dashboard data loaded successfully");
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("DashboardPage: Dashboard data loading was canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DashboardPage: Error loading dashboard data: {ex.Message}");
                ShowError($"Error loading dashboard data: {ex.Message}");
            }
            finally
            {
                // Hide loading indicators
                LoadingRing.IsActive = false;
            }
        }


        private async Task LoadSensorStatusAsync(CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine("DashboardPage: LoadSensorStatusAsync called");

                // Get sensor status counts
                PrtgSensorStatus sensorStatus = await PrtgService.GetSensorStatusAsync(_connectionInfo, cancellationToken);

                // Update UI with sensor status
                SensorCountTextBlock.Text = sensorStatus.TotalSensors.ToString();
                DeviceCountTextBlock.Text = sensorStatus.TotalDevices.ToString();
                UpSensorsTextBlock.Text = sensorStatus.UpSensors.ToString();
                DownSensorsTextBlock.Text = sensorStatus.DownSensors.ToString();

                Debug.WriteLine($"DashboardPage: Sensor status loaded - Total: {sensorStatus.TotalSensors}, Up: {sensorStatus.UpSensors}, Down: {sensorStatus.DownSensors}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DashboardPage: Error loading sensor status: {ex.Message}");
                throw;
            }
        }

        private async Task LoadSystemStatusAsync(CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine("DashboardPage: LoadSystemStatusAsync called");

                // Get system status
                var systemStatus = await PrtgService.GetSystemStatusAsync(_connectionInfo, cancellationToken);

                // Update UI with system status
                CpuLoadTextBlock.Text = systemStatus.CpuLoadValue;
                MemoryUsageTextBlock.Text = systemStatus.MemoryUsageValue;
                UptimeTextBlock.Text = systemStatus.UptimeValue;

                Debug.WriteLine($"DashboardPage: System status loaded - CPU: {systemStatus.CpuLoadValue}, Memory: {systemStatus.MemoryUsageValue}, Uptime: {systemStatus.UptimeValue}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DashboardPage: Error loading system status: {ex.Message}");
                throw;
            }
        }

        private async Task LoadRecentAlertsAsync(CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine("DashboardPage: LoadRecentAlertsAsync called");

                // Get recent alerts
                List<PrtgAlert> alerts = await PrtgService.GetRecentAlertsAsync(_connectionInfo, cancellationToken);

                // Update UI with alerts
                if (alerts.Count > 0)
                {
                    AlertsListView.ItemsSource = alerts.Select(a => new DashboardAlertViewModel(a)).ToList();
                    NoAlertsTextBlock.Visibility = Visibility.Collapsed;
                    AlertsListView.Visibility = Visibility.Visible;
                    Debug.WriteLine($"DashboardPage: {alerts.Count} alerts loaded");
                }
                else
                {
                    AlertsListView.ItemsSource = null;
                    NoAlertsTextBlock.Visibility = Visibility.Visible;
                    AlertsListView.Visibility = Visibility.Collapsed;
                    Debug.WriteLine("DashboardPage: No alerts found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DashboardPage: Error loading recent alerts: {ex.Message}");
                throw;
            }
        }

        private void ShowError(string message)
        {
            Debug.WriteLine($"DashboardPage: Showing error: {message}");
            ErrorInfoBar.Message = message;
            ErrorInfoBar.IsOpen = true;
        }
    }

    public sealed class DashboardAlertViewModel(PrtgAlert alert)
    {
        public string Message { get; } = alert.Message;
        public string TimeStamp { get; } = alert.TimeStamp;
        public string Status { get; } = alert.Status;
        public SolidColorBrush StatusColor { get; } = GetStatusColor(alert.Status);

        private static SolidColorBrush GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "up" => new SolidColorBrush(Color.FromArgb(255, 43, 123, 43)),      // Green
                "warning" => new SolidColorBrush(Color.FromArgb(255, 228, 155, 15)), // Orange
                "down" => new SolidColorBrush(Color.FromArgb(255, 209, 52, 56)),     // Red
                "paused" => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)), // Gray
                _ => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128))         // Gray
            };
        }
    }
}
