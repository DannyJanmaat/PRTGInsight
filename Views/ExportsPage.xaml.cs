using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PRTGInsight.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PRTGInsight.Views
{
    public sealed partial class ExportsPage : Page
    {
        public ExportsPage()
        {
            this.InitializeComponent();
            Loaded += ExportsPage_Loaded;
        }

        private async void ExportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRecentExportsAsync();
        }

        private async Task LoadRecentExportsAsync()
        {
            try
            {
                List<ExportInfo> exports = await ExportService.GetRecentExportsAsync();
                ExportsListView.ItemsSource = exports;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading recent exports: {ex.Message}");
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to load recent exports: {ex.Message}",
                    CloseButtonText = "OK"
                };
                _ = await dialog.ShowAsync();
            }
        }

        private async void ExportDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            await ExportDataAsync(ExportType.Dashboard);
        }

        private async void ExportSensorsButton_Click(object sender, RoutedEventArgs e)
        {
            await ExportDataAsync(ExportType.Sensors);
        }

        private async void ExportDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            await ExportDataAsync(ExportType.Devices);
        }

        private async void ExportAlertsButton_Click(object sender, RoutedEventArgs e)
        {
            await ExportDataAsync(ExportType.Alerts);
        }

        private async void CustomExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Show custom export dialog
            ContentDialog customExportDialog = new()
            {
                Title = "Custom Export",
                Content = new CustomExportDialog(),
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Export",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await customExportDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Get options from dialog and export
                CustomExportDialog customDialog = customExportDialog.Content as CustomExportDialog;
                if (customDialog != null)
                {
                    Dictionary<string, bool> options = customDialog.GetExportOptions();
                    await ExportDataAsync(ExportType.Custom, options);
                }
            }
        }

        private async Task ExportDataAsync(ExportType exportType, Dictionary<string, bool> customOptions = null)
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;

                bool success = false;
                switch (exportType)
                {
                    case ExportType.Dashboard:
                        success = await ExportService.ExportDashboardDataAsync(
                            ConnectionManager.CurrentConnection);
                        break;
                    case ExportType.Sensors:
                        success = await ExportService.ExportSensorsAsync(
                            ConnectionManager.CurrentConnection);
                        break;
                    case ExportType.Devices:
                        success = await ExportService.ExportDevicesAsync(
                            ConnectionManager.CurrentConnection);
                        break;
                    case ExportType.Alerts:
                        success = await ExportService.ExportAlertsAsync(
                            ConnectionManager.CurrentConnection);
                        break;
                    case ExportType.Custom:
                        success = await ExportService.CreateCustomExportAsync(
                            ConnectionManager.CurrentConnection,
                            customOptions);
                        break;
                }

                if (success)
                {
                    ContentDialog dialog = new()
                    {
                        Title = "Export Successful",
                        Content = $"The {exportType} data has been exported successfully.",
                        CloseButtonText = "OK"
                    };
                    _ = await dialog.ShowAsync();

                    // Refresh the list of exports
                    await LoadRecentExportsAsync();
                }
                else
                {
                    ContentDialog dialog = new()
                    {
                        Title = "Export Failed",
                        Content = $"Failed to export {exportType} data.",
                        CloseButtonText = "OK"
                    };
                    _ = await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting data: {ex.Message}");
                ContentDialog dialog = new()
                {
                    Title = "Export Error",
                    Content = $"An error occurred while exporting: {ex.Message}",
                    CloseButtonText = "OK"
                };
                _ = await dialog.ShowAsync();
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void RefreshExportsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadRecentExportsAsync();
        }

        private async void ExportsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExportsListView.SelectedItem is ExportInfo selectedExport)
            {
                try
                {
                    // Open the selected export file
                    bool success = await ExportService.OpenExportFileAsync(selectedExport.FilePath);

                    if (!success)
                    {
                        ContentDialog dialog = new()
                        {
                            Title = "Error",
                            Content = "Failed to open the export file.",
                            CloseButtonText = "OK"
                        };
                        _ = await dialog.ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error opening export file: {ex.Message}");
                    ContentDialog dialog = new()
                    {
                        Title = "Error",
                        Content = $"Failed to open the export file: {ex.Message}",
                        CloseButtonText = "OK"
                    };
                    _ = await dialog.ShowAsync();
                }
                finally
                {
                    // Clear selection
                    ExportsListView.SelectedItem = null;
                }
            }
        }
    }

    public partial class CustomExportDialog : StackPanel
    {
        private readonly CheckBox _sensorsCheckBox;
        private readonly CheckBox _devicesCheckBox;
        private readonly CheckBox _alertsCheckBox;
        private readonly CheckBox _systemStatusCheckBox;
        private readonly ComboBox _formatComboBox;

        public CustomExportDialog()
        {
            this.Spacing = 12;
            this.Margin = new Thickness(0, 12, 0, 0);

            // Add description
            TextBlock description = new()
            {
                Text = "Select the data you want to include in your custom export:",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            this.Children.Add(description);

            // Add options as checkboxes
            _sensorsCheckBox = new CheckBox { Content = "Sensors", IsChecked = true };
            _devicesCheckBox = new CheckBox { Content = "Devices", IsChecked = true };
            _alertsCheckBox = new CheckBox { Content = "Alerts", IsChecked = true };
            _systemStatusCheckBox = new CheckBox { Content = "System Status", IsChecked = true };

            this.Children.Add(_sensorsCheckBox);
            this.Children.Add(_devicesCheckBox);
            this.Children.Add(_alertsCheckBox);
            this.Children.Add(_systemStatusCheckBox);

            // Add export format selector
            TextBlock formatLabel = new()
            {
                Text = "Export Format:",
                Margin = new Thickness(0, 8, 0, 4)
            };
            this.Children.Add(formatLabel);

            _formatComboBox = new ComboBox
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _formatComboBox.Items.Add("CSV");
            _formatComboBox.Items.Add("JSON");
            _formatComboBox.Items.Add("XML");
            _formatComboBox.SelectedIndex = 0;
            this.Children.Add(_formatComboBox);
        }

        public Dictionary<string, bool> GetExportOptions()
        {
            return new Dictionary<string, bool>
        {
            { "Sensors", _sensorsCheckBox.IsChecked ?? false },
            { "Devices", _devicesCheckBox.IsChecked ?? false },
            { "Alerts", _alertsCheckBox.IsChecked ?? false },
            { "SystemStatus", _systemStatusCheckBox.IsChecked ?? false },
            { "Format", _formatComboBox.SelectedIndex == 0 }, // true for CSV, false for JSON
            { "UseXml", _formatComboBox.SelectedIndex == 2 }  // true for XML
        };
        }
    }

    public enum ExportType
    {
        Dashboard,
        Sensors,
        Devices,
        Alerts,
        Custom
    }

    public class ExportInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ExportType { get; set; }
        public string ExportDate { get; set; }
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted { get; set; }
    }
}
