using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using PRTGInsight.Services.Prtg;
using System;
using System.Threading.Tasks;
using Windows.UI;

namespace PRTGInsight.Views
{
    public sealed partial class SensorDetailsPage : Page
    {
        private readonly PrtgService _prtgService;
        private ConnectionInfo _connectionInfo;
        private int _sensorId;
        private PrtgSensorDetails _sensorDetails;

        public SensorDetailsPage()
        {
            this.InitializeComponent();
            _prtgService = new PrtgService();
            _connectionInfo = new ConnectionInfo();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
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
                    LoadSensorDetails();
                }
                else
                {
                    // Show error
                    ShowError("Invalid sensor ID provided.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnNavigatedTo: {ex.Message}");
                ShowError($"An error occurred: {ex.Message}");
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
                        Frame.Navigate(typeof(ConnectionPage));
                    }

                    return;
                }

                // Load sensor details now that we have connection info
                LoadSensorDetails();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading connection information: {ex.Message}");
            }
        }

        private async void LoadSensorDetails()
        {
            try
            {
                LoadingRing.IsActive = true;

                // Controleer of we verbindingsgegevens hebben
                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    LoadConnectionInfo();
                    return;
                }

                // Get sensor details
                _sensorDetails = await PrtgService.GetSensorDetailsAsync(_connectionInfo, _sensorId);

                // Update UI with sensor details
                UpdateUI();

                // Load graph image
                await LoadGraphImage();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading sensor details: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }

        private void UpdateUI()
        {
            try
            {
                if (_sensorDetails == null)
                    return;

                // Update header
                SensorNameTextBlock.Text = _sensorDetails.Name ?? "Unknown";

                // Update status
                StatusTextBlock.Text = _sensorDetails.Status ?? "Unknown";
                LastCheckTextBlock.Text = $"Last check: {_sensorDetails.LastCheck ?? "Unknown"}";

                // Update status indicator color
                Color statusColor = GetStatusColor(_sensorDetails.Status);
                StatusIndicator.Background = new SolidColorBrush(statusColor);

                // Update info cards
                LastValueTextBlock.Text = _sensorDetails.LastValue ?? "N/A";
                UptimeTextBlock.Text = _sensorDetails.Uptime ?? "N/A";
                DowntimeTextBlock.Text = _sensorDetails.Downtime ?? "N/A";

                // Update details
                DeviceTextBlock.Text = _sensorDetails.Device ?? "Unknown";
                TypeTextBlock.Text = _sensorDetails.Type ?? "Unknown";
                IntervalTextBlock.Text = _sensorDetails.Interval ?? "Unknown";
                PriorityTextBlock.Text = _sensorDetails.Priority.ToString();
                AccessRightsTextBlock.Text = _sensorDetails.AccessRights ?? "Unknown";
                TagsTextBlock.Text = _sensorDetails.Tags ?? "None";

                // Update message
                MessageTextBlock.Text = _sensorDetails.Message ?? "No message";

                // Update pause/resume button
                if (_sensorDetails.Status?.Equals("Paused", StringComparison.OrdinalIgnoreCase) == true)
                {
                    PauseResumeButton.Content = "Resume";
                }
                else
                {
                    PauseResumeButton.Content = "Pause";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating UI: {ex.Message}");
                ShowError($"Error updating UI: {ex.Message}");
            }
        }

        private async Task LoadGraphImage()
        {
            try
            {
                // Get graph URL
                string graphUrl = await PrtgService.GetSensorGraphUrlAsync(_connectionInfo, _sensorId);

                if (!string.IsNullOrEmpty(graphUrl))
                {
                    // Load the image
                    var bitmap = new BitmapImage(new Uri(graphUrl));
                    GraphImage.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                // Just log the error but don't show it to the user
                System.Diagnostics.Debug.WriteLine($"Error loading graph: {ex.Message}");
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

        private async void ScanNowButton_Click(object _, RoutedEventArgs __)
        {
            try
            {
                LoadingRing.IsActive = true;

                // Call the PRTG API to scan the sensor now
                bool success = await PrtgService.ScanSensorNowAsync(_connectionInfo, _sensorId);

                if (success)
                {
                    // Show success message
                    ContentDialog dialog = new()
                    {
                        Title = "Success",
                        Content = "Scan initiated successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();

                    // Reload sensor details after a short delay
                    await Task.Delay(2000);
                    LoadSensorDetails();
                }
                else
                {
                    // Show error
                    ShowError("Failed to initiate scan. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }

        private async void PauseResumeButton_Click(object _, RoutedEventArgs __)
        {
            try
            {
                LoadingRing.IsActive = true;

                bool success;

                if (_sensorDetails.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))
                {
                    // Resume sensor
                    success = await PrtgService.ResumeSensorAsync(_connectionInfo, _sensorId);
                }
                else
                {
                    // Pause sensor
                    success = await PrtgService.PauseSensorAsync(_connectionInfo, _sensorId, "Paused by PRTG Insight");
                }

                if (success)
                {
                    // Reload sensor details
                    LoadSensorDetails();
                }
                else
                {
                    // Show error
                    ShowError("Failed to pause/resume the sensor. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }

        private void HistoryButton_Click(object _, RoutedEventArgs __)
        {
            // Navigate to history page
            Frame.Navigate(typeof(SensorHistoryPage), new Tuple<int, ConnectionInfo>(_sensorId, _connectionInfo));
        }

        private static Color GetStatusColor(string status)
        {
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
