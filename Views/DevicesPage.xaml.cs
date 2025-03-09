using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

namespace PRTGInsight.Views
{
    public sealed partial class DevicesPage : Page
    {
        private readonly PrtgService _prtgService;
        private ConnectionInfo _connectionInfo;
        private List<PrtgDevice> _allDevices;
        private List<PrtgDevice> _filteredDevices;

        public DevicesPage()
        {
            this.InitializeComponent();
            _prtgService = new PrtgService();
            _connectionInfo = new ConnectionInfo();
            _allDevices = [];
            _filteredDevices = [];

            this.Loaded += OnPageLoaded;
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
                // Try to load the connection info if not passed
                _ = LoadConnectionInfoAsync();
            }
        }

        private async Task LoadConnectionInfoAsync()
        {
            try
            {
                _connectionInfo = SettingsService.LoadConnectionInfo();

                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    // Show an error message
                    ContentDialog dialog = new()
                    {
                        Title = "Connection Error",
                        Content = "No connection information available. Please connect to a PRTG server.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await dialog.ShowAsync();

                    // Navigate back to the connection page
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                    }
                    else
                    {
                        _ = Frame.Navigate(typeof(ConnectionPage));
                    }
                }
            }
            catch (Exception ex)
            {
                // Show an error message
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to load connection information: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await dialog.ShowAsync();
            }
        }

        private async void OnPageLoaded(object _, RoutedEventArgs __)
        {
            // Check if we have connection info
            if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
            {
                await LoadConnectionInfoAsync();
            }

            // Only load devices if we have connection info
            if (_connectionInfo != null && !string.IsNullOrEmpty(_connectionInfo.ServerUrl))
            {
                await LoadDevicesAsync();
            }
        }

        private void ShowConnectionErrorDialog()
        {
            try
            {
                ContentDialog dialog = new()
                {
                    Title = "Connection Error",
                    Content = "No connection information available. Please connect to a PRTG server.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }

        private async void RefreshButton_Click(object _, RoutedEventArgs __)
        {
            await LoadDevicesAsync();
        }

        private async void SearchBox_QuerySubmitted(object _, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            await LoadDevicesAsync(args.QueryText);
        }

        private void DevicesListView_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            if (DevicesListView?.SelectedItem is DeviceViewModel)
            {
                // Navigate to device details or show sensors for this device
                _ = Frame.Navigate(typeof(SensorsPage), _connectionInfo);
            }
        }

        // Method required by RefreshService
        public async void RefreshData()
        {
            try
            {
                await LoadDevicesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RefreshData: {ex.Message}");
            }
        }

        private async Task LoadDevicesAsync(string searchQuery = "")
        {
            try
            {
                Debug.WriteLine("DevicesPage: LoadDevicesAsync called");

                // Check if we have connection info
                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    Debug.WriteLine("DevicesPage: No connection info, trying to load from ConnectionManager");
                    _connectionInfo = ConnectionManager.CurrentConnection;

                    // If we still don't have connection info, stop
                    if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                    {
                        Debug.WriteLine("DevicesPage: Still no connection info, showing error");
                        ShowConnectionErrorDialog();
                        return;
                    }
                }

                ConnectionManager.DebugConnectionInfo();

                // Check if UI elements are initialized
                if (LoadingRing == null || DevicesListView == null)
                {
                    // Log an error
                    Debug.WriteLine("DevicesPage: UI elements not initialized in LoadDevicesAsync");
                    return;
                }

                LoadingRing.IsActive = true;
                DevicesListView.Visibility = Visibility.Collapsed;

                // Get devices
                List<PrtgDevice> devices;

                Debug.WriteLine($"DevicesPage: Getting devices with UseApiKey={_connectionInfo.UseApiKey}");

                if (_connectionInfo.UseApiKey)
                {
                    // Using API key
                    devices = await PrtgService.GetDevicesAsync(_connectionInfo.ServerUrl, _connectionInfo.ApiKey, CancellationToken.None);
                }
                else
                {
                    // Using username/password
                    devices = await PrtgService.GetDevicesWithCredentialsAsync(_connectionInfo.ServerUrl, _connectionInfo.Username, _connectionInfo.Password, CancellationToken.None);
                }

                Debug.WriteLine($"DevicesPage: Retrieved {devices.Count} devices");

                // Check if devices is null
                if (devices == null)
                {
                    devices = [];
                    Debug.WriteLine("DevicesPage: Devices list was null, created empty list");
                }

                _allDevices = devices;

                // Apply search filter if provided
                _filteredDevices = _allDevices;
                if (!string.IsNullOrWhiteSpace(searchQuery) && SearchBox != null)
                {
                    SearchBox.Text = searchQuery;
                    _filteredDevices = [.. _filteredDevices.Where(d =>
                        d.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        d.Type.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))];
                    Debug.WriteLine($"DevicesPage: Applied search filter: {searchQuery}, filtered to {_filteredDevices.Count} devices");
                }

                // Convert to view models
                List<DeviceViewModel> deviceViewModels = [.. _filteredDevices.Select(d => new DeviceViewModel(d))];
                Debug.WriteLine($"DevicesPage: Created {deviceViewModels.Count} view models");

                // Update UI
                DevicesListView.ItemsSource = deviceViewModels;
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"DevicesPage: Error in LoadDevicesAsync: {ex.Message}");
                Debug.WriteLine($"DevicesPage: Exception details: {ex}");

                // Show an error message if the UI elements are initialized
                if (this.XamlRoot != null)
                {
                    try
                    {
                        ContentDialog dialog = new()
                        {
                            Title = "Error Loading Devices",
                            Content = $"An error occurred while loading devices: {ex.Message}",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };

                        _ = await dialog.ShowAsync();
                    }
                    catch (Exception dialogEx)
                    {
                        Debug.WriteLine($"DevicesPage: Error showing dialog: {dialogEx.Message}");
                    }
                }
            }
            finally
            {
                // Check if the UI elements are initialized before updating them
                if (LoadingRing != null && DevicesListView != null)
                {
                    LoadingRing.IsActive = false;
                    DevicesListView.Visibility = Visibility.Visible;
                    Debug.WriteLine("DevicesPage: UI updated after loading");
                }
            }
        }
    }

    public class DeviceViewModel(PrtgDevice device)
    {
        public int Id { get; } = device.Id;
        public string Name { get; } = device.Name ?? "Unknown";
        public string Status { get; } = device.Status ?? "Unknown";
        public string Type { get; } = device.Type ?? "Unknown";
        public string Message { get; } = device.Message ?? "No message";
        public bool IsActive { get; } = device.IsActive;
        public string Tags { get; } = device.Tags ?? "None";
        public int Priority { get; } = device.Priority;
        public Windows.UI.Color StatusColor => GetStatusColor(Status);

        private static Windows.UI.Color GetStatusColor(string status)
        {
            return status.ToLower() switch
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