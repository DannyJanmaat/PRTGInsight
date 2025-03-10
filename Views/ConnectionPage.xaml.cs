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

        // Event for a successful login
        public event EventHandler<ConnectionInfo> LoginSuccessful;

        public ConnectionPage()
        {
            this.InitializeComponent();
            _prtgService = new PrtgService();
            this.Loaded += OnPageLoaded;
            // Use Unloaded event instead of overriding OnUnloaded
            this.Unloaded += ConnectionPage_Unloaded;
        }

        private async void OnPageLoaded(object _, RoutedEventArgs __)
        {
            _isInitialized = true;
            UpdateAuthPanelsVisibility();

            // Load login history
            System.Collections.Generic.List<string> loginHistory = await LoginHistoryService.LoadLoginHistoryAsync();
            LoginHistoryListView.ItemsSource = loginHistory;
        }

        // Replace OnUnloaded with ConnectionPage_Unloaded event handler
        private void ConnectionPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up event handlers
            this.Loaded -= OnPageLoaded;
            this.Unloaded -= ConnectionPage_Unloaded;

            // Cancel any pending operations
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            Debug.WriteLine("ConnectionPage: Unloaded and cleaned up resources");
        }

        private void EnsureUIComponents()
        {
            if (LoadingProgressRing == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: LoadingProgressRing is null");
            }

            if (ConnectButton == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: ConnectButton is null");
            }

            if (CancelButton == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: CancelButton is null");
            }
        }

        private void AuthType_Changed(object _, RoutedEventArgs __)
        {
            if (_isInitialized)
            {
                UpdateAuthPanelsVisibility();
            }
        }

        private void UpdateAuthPanelsVisibility()
        {
            // Ensure UI elements exist before modifying visibility
            if (UsernamePasswordRadio != null && UsernamePasswordPanel != null && ApiKeyPanel != null)
            {
                bool useUsernamePassword = UsernamePasswordRadio.IsChecked == true;
                UsernamePasswordPanel.Visibility = useUsernamePassword ? Visibility.Visible : Visibility.Collapsed;
                ApiKeyPanel.Visibility = useUsernamePassword ? Visibility.Collapsed : Visibility.Visible;
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
                // Show loading indicator and disable buttons
                LoadingProgressRing.IsActive = true;
                ConnectButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                StatusInfoBar.IsOpen = false;
                SslWarningInfoBar.IsOpen = false;

                // Create cancellation token
                _cancellationTokenSource = new CancellationTokenSource();

                // Update status and test connection
                StatusInfoBar.Title = "Connecting...";
                StatusInfoBar.Message = "Attempting to connect to PRTG server...";
                StatusInfoBar.Severity = InfoBarSeverity.Informational;
                StatusInfoBar.IsOpen = true;

                // Perform connection test using appropriate method
                PrtgStatus status = useApiKey
                    ? await PrtgService.TestConnectionWithApiKeyAsync(serverUrl, apiKey, _cancellationTokenSource.Token)
                    : await PrtgService.TestConnectionAsync(serverUrl, username, password, _cancellationTokenSource.Token);

                // Display SSL warning if needed
                if (status.HasSslWarning)
                {
                    SslWarningInfoBar.IsOpen = true;
                }

                // If connected, prepare ConnectionInfo, save settings, and navigate
                if (status.IsConnected)
                {
                    StatusInfoBar.Title = "Connected Successfully";
                    StatusInfoBar.Message = $"PRTG Version: {status.Version}";
                    StatusInfoBar.Severity = InfoBarSeverity.Success;

                    // Create connection info based on input and test result
                    ConnectionInfo connectionInfo = new()
                    {
                        ServerUrl = serverUrl,
                        Username = username,
                        Password = password,
                        ApiKey = apiKey,
                        UseApiKey = useApiKey,
                        IgnoreSslErrors = true,
                        PrtgVersion = status.Version
                    };

                    Debug.WriteLine("ConnectionPage: Connection successful");
                    Debug.WriteLine($"ConnectionPage: Created connection info - URL={connectionInfo.ServerUrl}, UseApiKey={connectionInfo.UseApiKey}");

                    // Update ConnectionManager and save settings
                    ConnectionManager.CurrentConnection = connectionInfo;
                    await SaveConnectionInfoAsync(connectionInfo);
                    Debug.WriteLine("ConnectionPage: Connection info saved");

                    // Save username to login history if using username/password
                    if (!useApiKey && !string.IsNullOrEmpty(username))
                    {
                        await LoginHistoryService.AddUsernameToHistoryAsync(username);
                    }

                    ConnectionManager.DebugConnectionInfo();

                    // Add a brief delay for user feedback
                    await Task.Delay(1500);

                    // Find parent that might be a MainPage to handle successful login
                    DependencyObject parent = this;
                    MainPage mainPage = null;
                    while (parent != null)
                    {
                        parent = VisualTreeHelper.GetParent(parent);
                        if (parent is MainPage mp)
                        {
                            mainPage = mp;
                            break;
                        }
                    }

                    // Trigger login event if subscribed; otherwise, navigate directly
                    if (LoginSuccessful != null)
                    {
                        Debug.WriteLine("ConnectionPage: Triggering LoginSuccessful event");
                        LoginSuccessful.Invoke(this, connectionInfo);
                    }
                    else if (mainPage != null)
                    {
                        Debug.WriteLine("ConnectionPage: Found MainPage, calling NavigateToDashboard");
                        mainPage.NavigateToDashboard();
                    }
                    else
                    {
                        Debug.WriteLine("ConnectionPage: No subscribers and no MainPage, using fallback navigation");
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
                if (LoadingProgressRing != null) // Add null check for safety
                {
                    LoadingProgressRing.IsActive = false;
                }

                if (ConnectButton != null) // Add null check for safety
                {
                    ConnectButton.IsEnabled = true;
                }

                if (CancelButton != null) // Add null check for safety
                {
                    CancelButton.IsEnabled = false;
                }
            }
        }

        // In ConnectionPage.xaml.cs, modify the NavigateToDashboard method:

        private void NavigateToDashboard(ConnectionInfo connectionInfo)
        {
            try
            {
                // Use MainWindow's method to handle post-login navigation if available
                if (MainWindow.Current != null && MainWindow.Current.Content != null)
                {
                    System.Diagnostics.Debug.WriteLine("ConnectionPage: Using MainWindow.Current to navigate");

                    // Fixed: Cast the MainWindow properly
                    if (MainWindow.Current is MainWindow mainWindow)
                    {
                        // Call the method
                        mainWindow.HandleAutoLogin(connectionInfo);
                        return;
                    }
                }

                // Fallback: search for a parent Frame to navigate
                DependencyObject parent = this;
                while (parent is not null and not Microsoft.UI.Xaml.Controls.Frame)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is Frame parentFrame)
                {
                    Debug.WriteLine("ConnectionPage: Navigating using parent frame");
                    _ = parentFrame.Navigate(typeof(DashboardPage));
                    return;
                }
                Debug.WriteLine("ConnectionPage: Could not find a navigation frame");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConnectionPage: Error in NavigateToDashboard: {ex.Message}");
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