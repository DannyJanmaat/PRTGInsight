using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using ReportGenerator = PRTGInsight.Services.Reports.ReportGenerator;
using ReportFormat = PRTGInsight.Services.Reports.ReportFormat;
using ReportType = PRTGInsight.Services.Reports.ReportType;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.System;

namespace PRTGInsight.Views
{
    public sealed partial class ReportsPage : Page
    {
        private ConnectionInfo _connectionInfo;
        private ReportGenerator _reportGenerator;
        private string _lastGeneratedReportPath;

        public ReportsPage()
        {
            this.InitializeComponent();

            // Initialize date pickers to today and one week ago
            EndDatePicker.Date = DateTime.Today;
            StartDatePicker.Date = DateTime.Today.AddDays(-7);

            this.Loaded += OnPageLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ConnectionInfo connectionInfo)
            {
                _connectionInfo = connectionInfo;
                InitializeReportGenerator();
            }
            else
            {
                // Try to load connection info if not passed
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
                    // Show error
                    ContentDialog dialog = new()
                    {
                        Title = "Connection Error",
                        Content = "No connection information available. Please connect to a PRTG server.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();

                    // Navigate back to connection page
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                    }
                    else
                    {
                        Frame.Navigate(typeof(ConnectionPage));
                    }
                }
                else
                {
                    InitializeReportGenerator();
                }
            }
            catch (Exception ex)
            {
                // Show error
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to load connection information: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void InitializeReportGenerator()
        {
            if (_connectionInfo != null)
            {
                _reportGenerator = new ReportGenerator(_connectionInfo);
            }
        }

        private async void OnPageLoaded(object _, RoutedEventArgs __)
        {
            // Ensure connection info is loaded
            if (_connectionInfo == null)
            {
                await LoadConnectionInfoAsync();
            }
        }

        private void ReportTypeComboBox_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            // Show/hide the object selector based on report type
            if (ReportTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string reportType = selectedItem.Tag?.ToString();

                // Show object selector for Probe and Group reports
                if (reportType == "ProbeReport" || reportType == "GroupReport")
                {
                    ObjectSelectorPanel.Visibility = Visibility.Visible;

                    // Populate the object selector - this would be implemented to
                    // fetch probes or groups based on the report type
                    _ = LoadObjectsAsync(reportType);
                }
                else
                {
                    ObjectSelectorPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async Task LoadObjectsAsync(string reportType)
        {
            try
            {
                // Clear existing items
                ObjectComboBox.Items.Clear();

                // Load objects based on report type
                if (reportType == "ProbeReport")
                {
                    // This would need to be implemented to get probes from PRTG
                    // For now, just add some placeholder items
                    ObjectComboBox.Items.Add(new ComboBoxItem { Content = "Probe 1", Tag = "1" });
                    ObjectComboBox.Items.Add(new ComboBoxItem { Content = "Probe 2", Tag = "2" });
                }
                else if (reportType == "GroupReport")
                {
                    // This would need to be implemented to get groups from PRTG
                    // For now, just add some placeholder items
                    ObjectComboBox.Items.Add(new ComboBoxItem { Content = "Group 1", Tag = "3" });
                    ObjectComboBox.Items.Add(new ComboBoxItem { Content = "Group 2", Tag = "4" });
                }

                // Select the first item if available
                if (ObjectComboBox.Items.Count > 0)
                {
                    ObjectComboBox.SelectedIndex = 0;
                }

                // Add an await to prevent the CS1998 warning
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading objects: {ex.Message}");

                // Show error
                StatusInfoBar.Title = "Error";
                StatusInfoBar.Message = $"Failed to load objects: {ex.Message}";
                StatusInfoBar.IsOpen = true;
            }
        }

        private async void GenerateReportButton_Click(object _, RoutedEventArgs __)
        {
            try
            {
                if (_reportGenerator == null)
                {
                    InitializeReportGenerator();

                    if (_reportGenerator == null)
                    {
                        StatusInfoBar.Title = "Error";
                        StatusInfoBar.Message = "Connection information not available.";
                        StatusInfoBar.IsOpen = true;
                        return;
                    }
                }

                // Get report options
                if (ReportTypeComboBox.SelectedItem is not ComboBoxItem reportTypeItem ||
                    FormatComboBox.SelectedItem is not ComboBoxItem formatItem)
                {
                    StatusInfoBar.Title = "Error";
                    StatusInfoBar.Message = "Please select report type and format.";
                    StatusInfoBar.IsOpen = true;
                    return;
                }

                // Parse report type
                string reportTypeString = reportTypeItem.Tag?.ToString();
                if (!Enum.TryParse<ReportType>(reportTypeString, out ReportType reportType))
                {
                    StatusInfoBar.Title = "Error";
                    StatusInfoBar.Message = "Invalid report type.";
                    StatusInfoBar.IsOpen = true;
                    return;
                }

                // Parse format
                string formatString = formatItem.Tag?.ToString();
                if (!Enum.TryParse<ReportFormat>(formatString, out ReportFormat format))
                {
                    StatusInfoBar.Title = "Error";
                    StatusInfoBar.Message = "Invalid report format.";
                    StatusInfoBar.IsOpen = true;
                    return;
                }

                // Get date range
                DateTime startDate = StartDatePicker.Date.Date;
                DateTime endDate = EndDatePicker.Date.Date.AddDays(1).AddSeconds(-1); // End of the selected day

                // Get object ID if applicable
                int? objectId = null;
                if (ObjectSelectorPanel.Visibility == Visibility.Visible &&
                    ObjectComboBox.SelectedItem is ComboBoxItem objectItem)
                {
                    string objectIdString = objectItem.Tag?.ToString();
                    if (int.TryParse(objectIdString, out int id))
                    {
                        objectId = id;
                    }
                }

                // Show loading state
                LoadingRing.IsActive = true;
                GenerateReportButton.IsEnabled = false;
                ReportSuccessPanel.Visibility = Visibility.Collapsed;
                NoPreviewTextBlock.Visibility = Visibility.Collapsed;
                StatusInfoBar.IsOpen = false;

                // Generate the report
                string reportPath = await _reportGenerator.GenerateReportAsync(
                    reportType, format, startDate, endDate, objectId);

                if (!string.IsNullOrEmpty(reportPath))
                {
                    // Store the report path
                    _lastGeneratedReportPath = reportPath;

                    // Show success message
                    ReportSuccessTextBlock.Text = $"Report has been saved to:\n{reportPath}";
                    ReportSuccessPanel.Visibility = Visibility.Visible;
                    NoPreviewTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // User cancelled or error occurred
                    NoPreviewTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating report: {ex.Message}");

                // Show error
                StatusInfoBar.Title = "Error";
                StatusInfoBar.Message = $"Failed to generate report: {ex.Message}";
                StatusInfoBar.IsOpen = true;

                NoPreviewTextBlock.Visibility = Visibility.Visible;
            }
            finally
            {
                // Reset UI state
                LoadingRing.IsActive = false;
                GenerateReportButton.IsEnabled = true;
            }
        }

        private async void OpenReportButton_Click(object _, RoutedEventArgs __)
        {
            if (!string.IsNullOrEmpty(_lastGeneratedReportPath) && File.Exists(_lastGeneratedReportPath))
            {
                try
                {
                    // Launch the file with the default application
                    await Launcher.LaunchFileAsync(await Windows.Storage.StorageFile.GetFileFromPathAsync(_lastGeneratedReportPath));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error opening report: {ex.Message}");

                    // Show error
                    StatusInfoBar.Title = "Error";
                    StatusInfoBar.Message = $"Failed to open report: {ex.Message}";
                    StatusInfoBar.IsOpen = true;
                }
            }
            else
            {
                StatusInfoBar.Title = "Error";
                StatusInfoBar.Message = "Report file not found.";
                StatusInfoBar.IsOpen = true;
            }
        }
    }
}