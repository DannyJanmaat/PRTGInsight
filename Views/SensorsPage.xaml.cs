using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using PRTGInsight.Services.Prtg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// Use the full namespace for Color to avoid ambiguity
using Windows.UI;

namespace PRTGInsight.Views
{
    public sealed partial class SensorsPage : Page
    {
        private readonly PrtgService _prtgService;
        private ConnectionInfo _connectionInfo;
        private List<PrtgSensor> _allSensors;
        private ObservableCollection<SensorViewModel> _filteredSensors;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private const int _pageSize = 20;
        private List<string> _deviceNames;

        public SensorsPage()
        {
            this.InitializeComponent();
            _prtgService = new PrtgService();
            _connectionInfo = new ConnectionInfo();
            _allSensors = [];
            _filteredSensors = [];
            _deviceNames = [];

            this.Loaded += OnPageLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);

                Debug.WriteLine("SensorsPage: OnNavigatedTo called");

                // Use the connection info from ConnectionManager
                _connectionInfo = ConnectionManager.CurrentConnection;

                ConnectionManager.DebugConnectionInfo();

                if (_connectionInfo != null)
                {
                    Debug.WriteLine($"SensorsPage: Using connection from ConnectionManager");
                }
                else
                {
                    Debug.WriteLine("SensorsPage: No connection info available");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnNavigatedTo: {ex.Message}");
            }
        }

        private async Task LoadConnectionInfoAsync()
        {
            try
            {
                _connectionInfo = SettingsService.LoadConnectionInfo();

                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    Debug.WriteLine("SensorsPage: No connection info found in settings");

                    // Wait until UI is fully loaded before showing a dialog
                    await Task.Delay(500);

                    // Show an error message if the UI is fully loaded
                    if (this.XamlRoot != null)
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
                            _ = await dialog.ShowAsync();
                        }
                        catch (Exception dialogEx)
                        {
                            Debug.WriteLine($"Error showing dialog: {dialogEx.Message}");
                        }

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
                else
                {
                    Debug.WriteLine($"SensorsPage: Loaded connection info from settings: {_connectionInfo.ServerUrl}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading connection info: {ex.Message}");
            }
        }

        private async void OnPageLoaded(object _, RoutedEventArgs __)
        {
            try
            {
                Debug.WriteLine("SensorsPage: OnPageLoaded called");

                // Ensure connection info is loaded
                if (_connectionInfo == null)
                {
                    Debug.WriteLine("SensorsPage: Connection info is null, getting from ConnectionManager");
                    _connectionInfo = ConnectionManager.CurrentConnection;
                    ConnectionManager.DebugConnectionInfo();
                }

                // Only load sensors if we have connection info
                if (_connectionInfo != null && !string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    Debug.WriteLine("SensorsPage: Loading sensors");
                    await LoadSensorsAsync();
                }
                else
                {
                    Debug.WriteLine("SensorsPage: No connection info available, showing error");
                    ShowConnectionErrorDialog();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnPageLoaded: {ex.Message}");
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
            await LoadSensorsAsync();
        }

        private async void SearchBox_QuerySubmitted(object _, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            await LoadSensorsAsync(args.QueryText);
        }

        private async void StatusFilter_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            await LoadSensorsAsync();
        }

        private void DeviceFilter_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            ApplyFilters();
        }

        private void SortBy_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            ApplySorting();
        }

        private void SensorsListView_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            try
            {
                if (SensorsListView?.SelectedItem is SensorViewModel selectedSensor)
                {
                    // Navigate to sensor details
                    _ = Frame.Navigate(typeof(SensorDetailsPage), new Tuple<int, ConnectionInfo>(selectedSensor.Id, _connectionInfo));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SensorsListView_SelectionChanged: {ex.Message}");

                // Show an error message
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        private void SensorActions_Click(object sender, RoutedEventArgs e)
        {
            // Get the sensor associated with this button
            if (sender is Button button && button.DataContext is SensorViewModel sensor)
            {
                ShowSensorActionsMenu(button, sensor);
            }
        }

        private async void ShowSensorActionsMenu(Button button, SensorViewModel sensor)
        {
            await Task.Run(() =>
            {
                // Create a menu with actions
                MenuFlyout menu = new();

                // Add menu items based on sensor status
                if (sensor.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))
                {
                    MenuFlyoutItem resumeItem = new() { Text = "Resume", Icon = new FontIcon { Glyph = "\uE768" } };
                    resumeItem.Click += (s, e) => ResumeSensor(sensor.Id);
                    menu.Items.Add(resumeItem);
                }
                else
                {
                    MenuFlyoutItem pauseItem = new() { Text = "Pause", Icon = new FontIcon { Glyph = "\uE769" } };
                    pauseItem.Click += (s, e) => PauseSensor(sensor.Id);
                    menu.Items.Add(pauseItem);
                }

                MenuFlyoutItem detailsItem = new() { Text = "View Details", Icon = new FontIcon { Glyph = "\uE8A0" } };
                detailsItem.Click += (s, e) => ViewSensorDetails(sensor.Id);
                menu.Items.Add(detailsItem);

                MenuFlyoutItem scanItem = new() { Text = "Scan Now", Icon = new FontIcon { Glyph = "\uE72C" } };
                scanItem.Click += (s, e) => ScanSensorNow(sensor.Id);
                menu.Items.Add(scanItem);

                // Show the menu
                menu.ShowAt(button);
            });
        }

        private async void PauseSensor(int sensorId)
        {
            try
            {
                if (LoadingRing != null)
                {
                    LoadingRing.IsActive = true;
                }

                // Call the PRTG API to pause the sensor
                bool success = await PrtgService.PauseSensorAsync(_connectionInfo, sensorId, "Paused by PRTG Insight");

                if (success)
                {
                    // Refresh the sensors list
                    await LoadSensorsAsync();
                }
                else
                {
                    // Show error
                    ContentDialog dialog = new()
                    {
                        Title = "Error",
                        Content = "Failed to pause the sensor. Please try again.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                // Show error
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await dialog.ShowAsync();
            }
            finally
            {
                if (LoadingRing != null)
                {
                    LoadingRing.IsActive = false;
                }
            }
        }

        private async void ResumeSensor(int sensorId)
        {
            try
            {
                if (LoadingRing != null)
                {
                    LoadingRing.IsActive = true;
                }

                // Call the PRTG API to resume the sensor
                bool success = await PrtgService.ResumeSensorAsync(_connectionInfo, sensorId);

                if (success)
                {
                    // Refresh the sensors list
                    await LoadSensorsAsync();
                }
                else
                {
                    // Show error
                    ContentDialog dialog = new()
                    {
                        Title = "Error",
                        Content = "Failed to resume the sensor. Please try again.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                // Show error
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await dialog.ShowAsync();
            }
            finally
            {
                if (LoadingRing != null)
                {
                    LoadingRing.IsActive = false;
                }
            }
        }

        private void ViewSensorDetails(int sensorId)
        {
            // Navigate to sensor details page
            _ = Frame.Navigate(typeof(SensorDetailsPage), new Tuple<int, ConnectionInfo>(sensorId, _connectionInfo));
        }

        // In ScanSensorNow method
        private async void ScanSensorNow(int sensorId)
        {
            try
            {
                // Create a local copy of the LoadingRing reference to ensure it doesn't change
                var loadingRing = LoadingRing;

                if (loadingRing != null)
                {
                    // Use the dispatcher to ensure UI updates happen on UI thread
                    _ = this.DispatcherQueue.TryEnqueue(() => {
                        loadingRing.IsActive = true;
                    });
                }

                // Use a cancelled token to prevent abandoned tasks
                using var cts = new CancellationTokenSource();

                // Call the PRTG API to scan the sensor now
                bool success = await PrtgService.ScanSensorNowAsync(_connectionInfo, sensorId);

                // Check if we're still in a valid UI state
                if (this.XamlRoot == null) return;

                if (success)
                {
                    // Show success message with proper error handling around dialog
                    try
                    {
                        ContentDialog dialog = new()
                        {
                            Title = "Success",
                            Content = "Scan initiated successfully.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        _ = await dialog.ShowAsync();
                    }
                    catch (Exception dialogEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Dialog error: {dialogEx.Message}");
                    }
                }
                else
                {
                    // Show error
                    try
                    {
                        ContentDialog dialog = new()
                        {
                            Title = "Error",
                            Content = "Failed to initiate scan. Please try again.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        _ = await dialog.ShowAsync();
                    }
                    catch (Exception dialogEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Dialog error: {dialogEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ScanSensorNow: {ex}");
            }
            finally
            {
                // Update UI state safely
                var loadingRing = LoadingRing;
                if (loadingRing != null)
                {
                    _ = this.DispatcherQueue.TryEnqueue(() => {
                        loadingRing.IsActive = false;
                    });
                }
            }
        }

        private void PreviousPage_Click(object _, RoutedEventArgs __)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
                LoadSensorsForCurrentPage();
            }
        }

        private void NextPage_Click(object _, RoutedEventArgs __)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdatePagination();
                LoadSensorsForCurrentPage();
            }
        }

        private void UpdatePagination()
        {
            if (PaginationText == null || PreviousPageButton == null || NextPageButton == null)
            {
                return;
            }

            PaginationText.Text = $"Page {_currentPage} of {_totalPages}";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;
        }

        private void ApplyFilters()
        {
            // Reset pagination
            _currentPage = 1;

            // Apply all filters and reload data
            FilterSensors();
            UpdatePagination();
            LoadSensorsForCurrentPage();
        }

        private void ApplySorting()
        {
            if (SortByComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                string sortBy = selectedItem.Content.ToString();

                switch (sortBy)
                {
                    case "Name":
                        _filteredSensors = [.. _filteredSensors.OrderBy(s => s.Name)];
                        break;
                    case "Status":
                        _filteredSensors = [.. _filteredSensors.OrderBy(s => s.Status)];
                        break;
                    case "Last Check":
                        _filteredSensors = [.. _filteredSensors.OrderByDescending(s => s.LastCheck)];
                        break;
                    case "Last Value":
                        _filteredSensors = [.. _filteredSensors.OrderBy(s => s.LastValue)];
                        break;
                }

                // Update the ListView
                LoadSensorsForCurrentPage();
            }
        }

        // This method is needed by the RefreshService
        public async void RefreshData()
        {
            try
            {
                await LoadSensorsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RefreshData: {ex.Message}");
            }
        }

        private async Task LoadSensorsAsync(string searchQuery = "")
        {
            try
            {
                Debug.WriteLine("SensorsPage: LoadSensorsAsync called");

                // Ensure connection info is loaded
                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    Debug.WriteLine("SensorsPage: No connection info, trying to load from ConnectionManager");
                    _connectionInfo = ConnectionManager.CurrentConnection;

                    // If still no connection info, show error
                    if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                    {
                        Debug.WriteLine("SensorsPage: Still no connection info, showing error");
                        ShowConnectionErrorDialog();
                        return;
                    }
                }

                ConnectionManager.DebugConnectionInfo();

                // Ensure UI elements are initialized
                if (LoadingRing == null || SensorsListView == null || DeviceFilter == null || SearchBox == null)
                {
                    Debug.WriteLine("SensorsPage: UI elements not initialized in LoadSensorsAsync");
                    return;
                }

                LoadingRing.IsActive = true;
                SensorsListView.Visibility = Visibility.Collapsed;

                // Get sensors
                List<PrtgSensor> sensors;

                Debug.WriteLine($"SensorsPage: Getting sensors with UseApiKey={_connectionInfo.UseApiKey}");

                if (_connectionInfo.UseApiKey)
                {
                    // Using API key
                    sensors = await PrtgService.GetSensorsAsync(_connectionInfo, CancellationToken.None);
                }
                else
                {
                    // Using username/password
                    sensors = await PrtgService.GetSensorsWithCredentialsAsync(_connectionInfo.ServerUrl, _connectionInfo.Username, _connectionInfo.Password, CancellationToken.None);
                }

                Debug.WriteLine($"SensorsPage: Retrieved {sensors.Count} sensors");

                // Ensure sensors is not null
                if (sensors == null)
                {
                    sensors = [];
                    Debug.WriteLine("SensorsPage: Sensors list was null, created empty list");
                }

                _allSensors = sensors;

                // Get unique device names for the filter
                _deviceNames = [.. _allSensors.Select(s => s.Device).Distinct().OrderBy(d => d)];
                DeviceFilter.ItemsSource = _deviceNames;
                Debug.WriteLine($"SensorsPage: Found {_deviceNames.Count} unique device names");

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    SearchBox.Text = searchQuery;
                    Debug.WriteLine($"SensorsPage: Applied search filter: {searchQuery}");
                }

                // Apply all filters
                FilterSensors();

                // Calculate pagination
                _totalPages = Math.Max(1, (_filteredSensors.Count + _pageSize - 1) / _pageSize);
                _currentPage = 1;
                UpdatePagination();
                Debug.WriteLine($"SensorsPage: Pagination set up: {_currentPage} of {_totalPages} pages");

                // Load first page
                LoadSensorsForCurrentPage();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SensorsPage: Error in LoadSensorsAsync: {ex.Message}");
                Debug.WriteLine($"SensorsPage: Exception details: {ex}");

                if (this.XamlRoot != null)
                {
                    try
                    {
                        ContentDialog dialog = new()
                        {
                            Title = "Error Loading Sensors",
                            Content = $"An error occurred while loading sensors: {ex.Message}",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };

                        _ = await dialog.ShowAsync();
                    }
                    catch (Exception dialogEx)
                    {
                        Debug.WriteLine($"SensorsPage: Error showing dialog: {dialogEx.Message}");
                    }
                }
            }
            finally
            {
                if (LoadingRing != null && SensorsListView != null)
                {
                    LoadingRing.IsActive = false;
                    SensorsListView.Visibility = Visibility.Visible;
                    Debug.WriteLine("SensorsPage: UI updated after loading");
                }
            }
        }

        private void FilterSensors()
        {
            if (_allSensors == null)
            {
                return;
            }

            IEnumerable<PrtgSensor> filteredSensors = _allSensors;

            // Apply search filter
            if (SearchBox != null)
            {
                string searchText = SearchBox.Text;
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredSensors = filteredSensors.Where(s =>
                        s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        s.Device.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Apply status filter
            if (StatusFilter?.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "All")
            {
                string statusFilter = statusItem.Content.ToString();
                filteredSensors = filteredSensors.Where(s => s.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Apply device filter
            if (DeviceFilter?.SelectedItem is string deviceName)
            {
                filteredSensors = filteredSensors.Where(s => s.Device.Equals(deviceName, StringComparison.OrdinalIgnoreCase));
            }

            // Convert to view models
            _filteredSensors = [.. filteredSensors.Select(s => new SensorViewModel(s))];
        }

        private void LoadSensorsForCurrentPage()
        {
            if (SensorsListView == null || _filteredSensors == null)
            {
                return;
            }

            // Get sensors for the current page
            List<SensorViewModel> pagedSensors = [.. _filteredSensors
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)];

            // Update the ListView
            SensorsListView.ItemsSource = pagedSensors;
        }
    }

    public class SensorViewModel
    {
        public int Id { get; }
        public string Name { get; }
        public string Status { get; }
        public string Device { get; }
        public string LastValue { get; }
        public DateTime LastCheck { get; }
        public string LastCheckFormatted => FormatTimeAgo(LastCheck);
        public Color StatusColor => GetStatusColor(Status);

        public SensorViewModel(PrtgSensor sensor)
        {
            Id = sensor.Id;
            Name = sensor.Name ?? "Unknown";
            Status = sensor.Status ?? "Unknown";
            Device = sensor.Device ?? "Unknown";
            LastValue = sensor.LastValue ?? "N/A";

            // Use a try-catch to handle problems with LastCheckDateTime
            try
            {
                LastCheck = sensor.LastCheckDateTime;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting LastCheckDateTime: {ex.Message}");
                LastCheck = DateTime.Now;
            }
        }

        private static string FormatTimeAgo(DateTime dateTime)
        {
            try
            {
                TimeSpan timeSpan = DateTime.Now - dateTime;

                return timeSpan.TotalMinutes < 1
                    ? "Just now"
                    : timeSpan.TotalMinutes < 60
                    ? $"{(int)timeSpan.TotalMinutes} min ago"
                    : timeSpan.TotalHours < 24 ? $"{(int)timeSpan.TotalHours} hours ago" : dateTime.ToString("MMM dd, HH:mm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting time ago: {ex.Message}");
                return "Unknown";
            }
        }

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
                "paused" => Color.FromArgb(255, 128, 128, 128), // Gray
                _ => Color.FromArgb(255, 128, 128, 128)         // Gray
            };
        }
    }
}