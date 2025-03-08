using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using PRTGInsight.Services.Prtg;
using System;
using System.Linq;

namespace PRTGInsight.Views
{
    public sealed partial class SensorHistoryPage : Page
    {
        private readonly PrtgService _prtgService;
        private ConnectionInfo _connectionInfo;
        private int _sensorId;
        private string _sensorName;

        public SensorHistoryPage()
        {
            this.InitializeComponent();
            _prtgService = new PrtgService();
            _connectionInfo = new ConnectionInfo();

            // Set default date range (last 7 days)
            EndDatePicker.Date = DateTime.Now;
            StartDatePicker.Date = DateTime.Now.AddDays(-7);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int sensorId)
            {
                _sensorId = sensorId;
                LoadConnectionInfo();
            }
            else if (e.Parameter is Tuple<int, ConnectionInfo> parameters)
            {
                _sensorId = parameters.Item1;
                _connectionInfo = parameters.Item2;
                LoadSensorHistory();
            }
            else if (e.Parameter is Tuple<int, string> sensorInfo)
            {
                _sensorId = sensorInfo.Item1;
                _sensorName = sensorInfo.Item2;
                SensorNameTextBlock.Text = $"{_sensorName} - History";
                LoadConnectionInfo();
            }
            else
            {
                // Show error
                ShowError("Invalid sensor ID provided.");
            }
        }

        private void LoadConnectionInfo()
        {
            try
            {
                _connectionInfo = SettingsService.LoadConnectionInfo();

                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    // Toon een foutmelding
                    ShowError("Connection information not available. Please reconnect to the PRTG server.");

                    // Navigeer terug naar de verbindingspagina
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                    }
                    else
                    {
                        _ = Frame.Navigate(typeof(ConnectionPage));
                    }

                    return;
                }

                // Load sensor history now that we have connection info
                LoadSensorHistory();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading connection information: {ex.Message}");
            }
        }

        private async void LoadSensorHistory()
        {
            try
            {
                LoadingRing.IsActive = true;
                HistoryListView.Visibility = Visibility.Collapsed;
                NoDataTextBlock.Visibility = Visibility.Collapsed;

                // Controleer of we verbindingsgegevens hebben
                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    LoadConnectionInfo();
                    return;
                }

                // Get sensor history
                DateTime startDate = StartDatePicker.Date.Date;
                DateTime endDate = EndDatePicker.Date.Date.AddDays(1).AddSeconds(-1); // End of the selected day

                System.Collections.Generic.List<PrtgSensorHistoryItem> historyItems = await PrtgService.GetSensorHistoryAsync(_connectionInfo, _sensorId, startDate, endDate);

                if (historyItems.Count == 0)
                {
                    NoDataTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    // Convert to view models
                    System.Collections.Generic.List<HistoryItemViewModel> historyViewModels = [.. historyItems.Select(h => new HistoryItemViewModel(h))];

                    // Update UI
                    HistoryListView.ItemsSource = historyViewModels;
                    HistoryListView.Visibility = Visibility.Visible;
                }

                // If we don't have the sensor name yet, try to get it
                if (string.IsNullOrEmpty(_sensorName))
                {
                    try
                    {
                        PrtgSensorDetails sensorDetails = await PrtgService.GetSensorDetailsAsync(_connectionInfo, _sensorId);
                        _sensorName = sensorDetails.Name;
                        SensorNameTextBlock.Text = $"{_sensorName} - History";
                    }
                    catch
                    {
                        // Ignore errors here, we'll just use a generic title
                        SensorNameTextBlock.Text = $"Sensor {_sensorId} - History";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading sensor history: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }

        private void ShowError(string message)
        {
            // Show error dialog
            ContentDialog dialog = new()
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private void BackButton_Click(object _, RoutedEventArgs __)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ApplyButton_Click(object _, RoutedEventArgs __)
        {
            LoadSensorHistory();
        }
    }

    public class HistoryItemViewModel(PrtgSensorHistoryItem historyItem)
    {
        public string DateTime { get; } = historyItem.DateTime;
        public string DateTimeFormatted { get; } = historyItem.DateTimeValue.ToString("yyyy-MM-dd HH:mm:ss");
        public string Value { get; } = historyItem.Value;
        public string Status { get; } = historyItem.Status;
        public string Message { get; } = historyItem.Message;
        public Microsoft.UI.Xaml.Media.SolidColorBrush StatusColor { get; } = GetStatusColor(historyItem.Status);

        private static Microsoft.UI.Xaml.Media.SolidColorBrush GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "up" => new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 43, 123, 43)),      // Green
                "warning" => new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 228, 155, 15)), // Orange
                "down" => new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 209, 52, 56)),     // Red
                "paused" => new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128)), // Gray
                _ => new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128))         // Gray
            };
        }
    }
}
