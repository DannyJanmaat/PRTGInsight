using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PRTGInsight.Models;
using PRTGInsight.Services;
using PRTGInsight.Services.Prtg;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PRTGInsight.Views
{
    public sealed partial class ConnectionPage : Page
    {
        private readonly PrtgService _prtgService;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isInitialized = false;

        // Event voor succesvolle login
        public event EventHandler<ConnectionInfo> LoginSuccessful;

        public ConnectionPage()
        {
            this.InitializeComponent();
            _prtgService = new PrtgService();

            // Add a Loaded event handler to set up initial state after all elements are loaded
            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object _, RoutedEventArgs __)
        {
            _isInitialized = true;

            // Set initial visibility state
            UpdateAuthPanelsVisibility();

            // Load login history
            System.Collections.Generic.List<string> loginHistory = await LoginHistoryService.LoadLoginHistoryAsync();
            LoginHistoryListView.ItemsSource = loginHistory;
        }

        private void AuthType_Changed(object _, RoutedEventArgs __)
        {
            // Only update visibility if the page is fully initialized
            if (_isInitialized)
            {
                UpdateAuthPanelsVisibility();
            }
        }

        private void UpdateAuthPanelsVisibility()
        {
            // Make sure all UI elements exist before accessing them
            if (UsernamePasswordRadio != null && UsernamePasswordPanel != null && ApiKeyPanel != null)
            {
                bool useUsernamePassword = UsernamePasswordRadio.IsChecked == true;

                UsernamePasswordPanel.Visibility = useUsernamePassword ?
                    Visibility.Visible : Visibility.Collapsed;

                ApiKeyPanel.Visibility = useUsernamePassword ?
                    Visibility.Collapsed : Visibility.Visible;
            }
        }

        private async void ConnectButton_Click(object _, RoutedEventArgs __)
        {
            await TestConnection();
        }

        private void CancelButton_Click(object _, RoutedEventArgs __)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void ToggleHistoryPane(object sender, RoutedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(sender);

            ArgumentNullException.ThrowIfNull(e);

            LoginSplitView.IsPaneOpen = !LoginSplitView.IsPaneOpen;
        }

        private void LoginHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(sender);

            ArgumentNullException.ThrowIfNull(e);

            if (LoginHistoryListView.SelectedItem is string selectedUsername)
            {
                UsernameTextBox.Text = selectedUsername;
                UsernamePasswordRadio.IsChecked = true;
                LoginSplitView.IsPaneOpen = false;
            }
        }

        private async Task TestConnection()
        {
            // Get values from UI
            string serverUrl = ServerUrlTextBox.Text;
            bool useApiKey = ApiKeyRadio.IsChecked == true;

            string username = useApiKey ? string.Empty : UsernameTextBox.Text;
            string password = useApiKey ? string.Empty : PasswordBox.Password;
            string apiKey = useApiKey ? ApiKeyBox.Password : string.Empty;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                StatusInfoBar.Message = "Please enter a server URL";
                StatusInfoBar.Severity = InfoBarSeverity.Warning;
                StatusInfoBar.IsOpen = true;
                return;
            }

            if (useApiKey && string.IsNullOrWhiteSpace(apiKey))
            {
                StatusInfoBar.Message = "Please enter an API Key";
                StatusInfoBar.Severity = InfoBarSeverity.Warning;
                StatusInfoBar.IsOpen = true;
                return;
            }
            if (!useApiKey && (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)))
            {
                StatusInfoBar.Message = "Please enter both username and password";
                StatusInfoBar.Severity = InfoBarSeverity.Warning;
                StatusInfoBar.IsOpen = true;
                return;
            }

            try
            {
                // Show loading indicator and enable cancel button
                LoadingRing.IsActive = true;
                ConnectButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                StatusInfoBar.IsOpen = false;

                // Create cancellation token
                _cancellationTokenSource = new CancellationTokenSource();

                // Test connection
                StatusInfoBar.Title = "Connecting...";
                StatusInfoBar.Message = "Attempting to connect to PRTG server...";
                StatusInfoBar.Severity = InfoBarSeverity.Informational;
                StatusInfoBar.IsOpen = true;

                PrtgStatus status = useApiKey
                    ? await PrtgService.TestConnectionWithApiKeyAsync(serverUrl, apiKey, _cancellationTokenSource.Token)
                    : await PrtgService.TestConnectionAsync(serverUrl, username, password, _cancellationTokenSource.Token);

                // Update UI based on result
                if (status.IsConnected)
                {
                    StatusInfoBar.Title = "Connected Successfully";
                    StatusInfoBar.Message = $"PRTG Version: {status.Version}";
                    StatusInfoBar.Severity = InfoBarSeverity.Success;

                    // Create connection info to pass to dashboard
                    ConnectionInfo connectionInfo = new()
                    {
                        ServerUrl = serverUrl,
                        Username = username,
                        Password = password,
                        ApiKey = apiKey,
                        UseApiKey = useApiKey,
                        IgnoreSslErrors = IgnoreSslCheckBox.IsChecked ?? true,
                        PrtgVersion = status.Version
                    };

                    Debug.WriteLine("ConnectionPage: Connection successful");
                    Debug.WriteLine($"ConnectionPage: Created connection info - URL={connectionInfo.ServerUrl}, UseApiKey={connectionInfo.UseApiKey}");

                    // Set the connection in the ConnectionManager
                    ConnectionManager.CurrentConnection = connectionInfo;

                    // Save connection info to settings
                    await SaveConnectionInfoAsync(connectionInfo);

                    Debug.WriteLine("ConnectionPage: Connection info saved to settings");

                    // Save username to login history if using username/password
                    if (!useApiKey && !string.IsNullOrEmpty(username))
                    {
                        await LoginHistoryService.AddUsernameToHistoryAsync(username);
                    }

                    // Debug the ConnectionManager
                    ConnectionManager.DebugConnectionInfo();

                    // Add a short delay to show the success message before navigating
                    await Task.Delay(1500);

                    // IMPORTANT: Make sure the event is actually triggered
                    // Check if there are any subscribers before invoking
                    if (LoginSuccessful != null)
                    {
                        Debug.WriteLine("ConnectionPage: Triggering LoginSuccessful event");
                        LoginSuccessful.Invoke(this, connectionInfo);
                    }
                    else
                    {
                        Debug.WriteLine("ConnectionPage: No subscribers to LoginSuccessful event");

                        // As a fallback, try to navigate directly
                        NavigateToDashboard(connectionInfo);
                    }
                }
                else
                {
                    StatusInfoBar.Title = "Connection Failed";
                    StatusInfoBar.Message = status.Message;
                    StatusInfoBar.Severity = InfoBarSeverity.Error;
                }
            }
            catch (Exception ex)
            {
                StatusInfoBar.Title = "Error";
                StatusInfoBar.Message = $"An unexpected error occurred: {ex.Message}";
                StatusInfoBar.Severity = InfoBarSeverity.Error;
                Debug.WriteLine($"ConnectionPage: Error in TestConnection: {ex.Message}");
                Debug.WriteLine($"ConnectionPage: Stack trace: {ex.StackTrace}");
            }
            finally
            {
                // Reset UI state
                LoadingRing.IsActive = false;
                ConnectButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }
        }

        private void NavigateToDashboard(ConnectionInfo connectionInfo)
        {
            try
            {
                // Try to find the main window and navigate
                if (MainWindow.Current != null)
                {
                    Debug.WriteLine("ConnectionPage: Using MainWindow.Current to navigate");
                    MainWindow.Current.HandleAutoLogin(connectionInfo);
                    return;
                }

                // Try to find parent frames or windows
                DependencyObject parent = this;
                while (parent is not null and not Microsoft.UI.Xaml.Controls.Frame)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is Frame parentFrame)
                {
                    Debug.WriteLine("ConnectionPage: Using parent frame to navigate");
                    _ = parentFrame.Navigate(typeof(DashboardPage));
                    return;
                }

                Debug.WriteLine("ConnectionPage: Could not find a way to navigate");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConnectionPage: Error in NavigateToDashboard: {ex.Message}");
            }
        }

        private static async Task SaveConnectionInfoAsync(ConnectionInfo connectionInfo)
        {
            try
            {
                await SettingsService.SaveConnectionInfoAsync(connectionInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving connection info: {ex.Message}");
            }
        }
    }
}