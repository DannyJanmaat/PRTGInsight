using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PRTGInsight.Models;
using PRTGInsight.Services;
using PRTGInsight.Services.Prtg;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace PRTGInsight.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Load current connection info
            LoadConnectionInfo();

            // Load app settings
            LoadAppSettings();

            // Calculate export folder size
            await UpdateExportFolderSizeAsync();

            // Set app version
            VersionTextBlock.Text = $"Version {GetAppVersion()}";
        }

        private void LoadConnectionInfo()
        {
            if (ConnectionManager.CurrentConnection != null)
            {
                ServerUrlTextBlock.Text = ConnectionManager.CurrentConnection.ServerUrl;
                AuthTypeTextBlock.Text = ConnectionManager.CurrentConnection.UseApiKey
                    ? "API Key"
                    : "Username/Password";
                PrtgVersionTextBlock.Text = ConnectionManager.CurrentConnection.PrtgVersion ?? "Unknown";
            }
            else
            {
                ServerUrlTextBlock.Text = "Not connected";
                AuthTypeTextBlock.Text = "--";
                PrtgVersionTextBlock.Text = "--";
            }
        }

        private void LoadAppSettings()
        {
            // Load theme setting
            string currentTheme = SettingsService.GetSetting("AppTheme", "Default");
            foreach (RadioButton rb in ThemeRadioButtons.Items.OfType<RadioButton>())
            {
                if (rb.Tag.ToString() == currentTheme)
                {
                    rb.IsChecked = true;
                    break;
                }
            }

            // Load auto refresh settings
            bool autoRefresh = SettingsService.GetSetting("AutoRefresh", false);
            AutoRefreshToggle.IsOn = autoRefresh;
            RefreshIntervalPanel.Visibility = autoRefresh ? Visibility.Visible : Visibility.Collapsed;

            // Load refresh interval
            int refreshInterval = SettingsService.GetSetting("RefreshInterval", 60);
            foreach (ComboBoxItem item in RefreshIntervalComboBox.Items.OfType<ComboBoxItem>())
            {
                if (int.Parse(item.Tag.ToString()) == refreshInterval)
                {
                    RefreshIntervalComboBox.SelectedItem = item;
                    break;
                }
            }

            // Load default export format
            string defaultFormat = SettingsService.GetSetting("DefaultExportFormat", "json");
            foreach (ComboBoxItem item in DefaultExportFormatComboBox.Items.OfType<ComboBoxItem>())
            {
                if (item.Tag.ToString() == defaultFormat)
                {
                    DefaultExportFormatComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private async Task UpdateExportFolderSizeAsync()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder exportFolder = await localFolder.CreateFolderAsync(
                    "Exports", CreationCollisionOption.OpenIfExists);

                System.Collections.Generic.IReadOnlyList<StorageFile> files = await exportFolder.GetFilesAsync();
                ulong totalSize = 0;

                foreach (StorageFile file in files)
                {
                    Windows.Storage.FileProperties.BasicProperties properties = await file.GetBasicPropertiesAsync();
                    totalSize += properties.Size;
                }

                ExportFolderSizeTextBlock.Text = $"Export folder size: {FormatFileSize(totalSize)}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating export folder size: {ex.Message}");
                ExportFolderSizeTextBlock.Text = "Export folder size: Unknown";
            }
        }

        private string GetAppVersion()
        {
            try
            {
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        private string FormatFileSize(ulong sizeInBytes)
        {
            string[] sizes = ["B", "KB", "MB", "GB"];
            double formattedSize = sizeInBytes;
            int sizeIndex = 0;

            while (formattedSize >= 1024 && sizeIndex < sizes.Length - 1)
            {
                formattedSize /= 1024;
                sizeIndex++;
            }

            return $"{formattedSize:0.##} {sizes[sizeIndex]}";
        }

        private void ChangeConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to connection page
            _ = Frame.Navigate(typeof(ConnectionPage));
        }

        // Fix for the TestConnectionButton_Click method in SettingsPage.xaml.cs
        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectionManager.CurrentConnection == null)
            {
                ContentDialog dialog = new()
                {
                    Title = "No Connection",
                    Content = "No connection is currently configured.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await dialog.ShowAsync();
                return;
            }

            try
            {
                // Show loading indicator
                TestConnectionButton.IsEnabled = false;
                TestConnectionButton.Content = "Testing...";

                // Create a cancellation token
                using CancellationTokenSource cts = new();
                cts.CancelAfter(TimeSpan.FromSeconds(30)); // Set a timeout

                // Test the connection
                PrtgStatus status = ConnectionManager.CurrentConnection.UseApiKey
                    ? await PrtgService.TestConnectionWithApiKeyAsync(
                        ConnectionManager.CurrentConnection.ServerUrl,
                        ConnectionManager.CurrentConnection.ApiKey,
                        cts.Token)
                    : await PrtgService.TestConnectionAsync(
                        ConnectionManager.CurrentConnection.ServerUrl,
                        ConnectionManager.CurrentConnection.Username,
                        ConnectionManager.CurrentConnection.Password,
                        cts.Token);

                // Show result
                if (status.IsConnected)
                {
                    ContentDialog dialog = new()
                    {
                        Title = "Connection Successful",
                        Content = $"Successfully connected to PRTG server.\nVersion: {status.Version}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await dialog.ShowAsync();
                }
                else
                {
                    ContentDialog dialog = new()
                    {
                        Title = "Connection Failed",
                        Content = $"Failed to connect to PRTG server: {status.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = await dialog.ShowAsync();
                }
            }
            catch (OperationCanceledException)
            {
                ContentDialog dialog = new()
                {
                    Title = "Connection Timeout",
                    Content = "The connection test timed out. Please check your network connection and try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
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
                // Reset button
                TestConnectionButton.IsEnabled = true;
                TestConnectionButton.Content = "Test Connection";
            }
        }

        private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeRadioButtons.SelectedItem is RadioButton selectedButton)
            {
                string theme = selectedButton.Tag.ToString();
                SettingsService.SaveSetting("AppTheme", theme);

                // Apply theme
                if (theme == "Light")
                {
                    if (Window.Current.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = ElementTheme.Light;
                    }
                }
                else if (theme == "Dark")
                {
                    if (Window.Current.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = ElementTheme.Dark;
                    }
                }
                else // Default
                {
                    if (Window.Current.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = ElementTheme.Default;
                    }
                }
            }
        }

        private void AutoRefreshToggle_Toggled(object sender, RoutedEventArgs e)
        {
            bool isOn = AutoRefreshToggle.IsOn;
            SettingsService.SaveSetting("AutoRefresh", isOn);
            RefreshIntervalPanel.Visibility = isOn ? Visibility.Visible : Visibility.Collapsed;

            // Apply auto refresh setting
            if (isOn)
            {
                // Start auto refresh
                int interval = int.Parse((RefreshIntervalComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "60");
                RefreshService.StartAutoRefresh(interval);
            }
            else
            {
                // Stop auto refresh
                RefreshService.StopAutoRefresh();
            }
        }

        private void RefreshIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RefreshIntervalComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int interval = int.Parse(selectedItem.Tag.ToString());
                SettingsService.SaveSetting("RefreshInterval", interval);

                // Update auto refresh if enabled
                if (AutoRefreshToggle.IsOn)
                {
                    RefreshService.UpdateRefreshInterval(interval);
                }
            }
        }

        private void DefaultExportFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultExportFormatComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string format = selectedItem.Tag.ToString();
                SettingsService.SaveSetting("DefaultExportFormat", format);
            }
        }

        private async void ClearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog confirmDialog = new()
            {
                Title = "Clear Cache",
                Content = "Are you sure you want to clear the app cache? This will remove all temporary data.",
                PrimaryButtonText = "Clear",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    // Fully qualify CacheService to avoid ambiguity
                    await PRTGInsight.Services.CacheService.ClearCacheAsync();

                    ContentDialog successDialog = new()
                    {
                        Title = "Cache Cleared",
                        Content = "The app cache has been cleared successfully.",
                        CloseButtonText = "OK"
                    };
                    _ = await successDialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new()
                    {
                        Title = "Error",
                        Content = $"Failed to clear cache: {ex.Message}",
                        CloseButtonText = "OK"
                    };
                    _ = await errorDialog.ShowAsync();
                }
            }
        }

        private async void ClearExportsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog confirmDialog = new()
            {
                Title = "Clear Exports",
                Content = "Are you sure you want to delete all export files? This action cannot be undone.",
                PrimaryButtonText = "Delete All",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    // Clear all export files
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    StorageFolder exportFolder = await localFolder.CreateFolderAsync("Exports", CreationCollisionOption.OpenIfExists);

                    System.Collections.Generic.IReadOnlyList<StorageFile> files = await exportFolder.GetFilesAsync();
                    foreach (StorageFile file in files)
                    {
                        await file.DeleteAsync();
                    }

                    // Update folder size
                    await UpdateExportFolderSizeAsync();

                    ContentDialog successDialog = new()
                    {
                        Title = "Exports Cleared",
                        Content = "All export files have been deleted successfully.",
                        CloseButtonText = "OK"
                    };
                    _ = await successDialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new()
                    {
                        Title = "Error",
                        Content = $"Failed to delete export files: {ex.Message}",
                        CloseButtonText = "OK"
                    };
                    _ = await errorDialog.ShowAsync();
                }
            }
        }

        private async void OpenExportFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder exportFolder = await localFolder.CreateFolderAsync("Exports", CreationCollisionOption.OpenIfExists);

                // Open the folder in File Explorer
                _ = await Launcher.LaunchFolderAsync(exportFolder);
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to open export folder: {ex.Message}",
                    CloseButtonText = "OK"
                };
                _ = await errorDialog.ShowAsync();
            }
        }

        private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show checking message
                CheckUpdatesButton.IsEnabled = false;
                CheckUpdatesButton.Content = "Checking...";

                // Check for updates (simulated)
                await Task.Delay(1500); // Simulate network request

                // Show result
                ContentDialog dialog = new()
                {
                    Title = "Updates",
                    Content = "You are using the latest version of PRTG Insight.",
                    CloseButtonText = "OK"
                };
                _ = await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to check for updates: {ex.Message}",
                    CloseButtonText = "OK"
                };
                _ = await dialog.ShowAsync();
            }
            finally
            {
                // Reset button
                CheckUpdatesButton.IsEnabled = true;
                CheckUpdatesButton.Content = "Check for Updates";
            }
        }
    }
}
