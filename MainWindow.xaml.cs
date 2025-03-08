using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using PRTGInsight.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WinRT.Interop;

namespace PRTGInsight
{
    public sealed partial class MainWindow : Window
    {
        private ConnectionInfo _currentConnectionInfo;
        private readonly Frame ContentFrame; // Add this line to declare ContentFrame

        // Add a static instance of the current window for access throughout the app
        public static new MainWindow Current { get; private set; }

        public MainWindow()
        {
            this.InitializeComponent();
            Title = "PRTG Insight";

            // Store the window instance for global access
            Current = this;

            // Set the window icon
            SetWindowIcon();

            // Ensure LoginFrame is initialized
            if (LoginFrame == null)
            {
                throw new InvalidOperationException("LoginFrame is not initialized.");
            }

            // Initialize ContentFrame
            ContentFrame = new Frame();
            ContentFrame.NavigationFailed += ContentFrame_NavigationFailed;

            // Register the content frame with appropriate event handlers
            NavView.Content = ContentFrame;

            // Load connection information from settings
            LoadConnectionInfoAsync();
        }

        private void LoadConnectionInfoAsync()
        {
            try
            {
                // Load connection info from file-based storage
                ConnectionInfo connectionInfo = SettingsService.LoadConnectionInfo();

                // Set it in the ConnectionManager if valid
                if (connectionInfo != null && !string.IsNullOrEmpty(connectionInfo.ServerUrl))
                {
                    ConnectionManager.CurrentConnection = connectionInfo;
                }

                // Check if we have connection info
                if (ConnectionManager.IsConnected)
                {
                    Debug.WriteLine("MainWindow: Found saved connection, auto-login");
                    ConnectionManager.DebugConnectionInfo();

                    // Auto-login with saved data
                    HandleAutoLogin(ConnectionManager.CurrentConnection);
                }
                else
                {
                    Debug.WriteLine("MainWindow: No saved connection, showing login page");

                    // Ensure LoginFrame is initialized before navigating
                    if (LoginFrame != null)
                    {
                        // Navigate to the login page
                        _ = LoginFrame.Navigate(typeof(ConnectionPage));

                        // Listen for the login event
                        if (LoginFrame.Content is ConnectionPage connectionPage)
                        {
                            // Remove any existing handlers to avoid duplicates
                            connectionPage.LoginSuccessful -= OnLoginSuccessful;
                            connectionPage.LoginSuccessful += OnLoginSuccessful;
                            Debug.WriteLine("MainWindow: Subscribed to LoginSuccessful event");
                        }
                        else
                        {
                            Debug.WriteLine("MainWindow: LoginFrame.Content is not ConnectionPage");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("MainWindow: LoginFrame is not initialized");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow: Error loading connection info: {ex.Message}");

                // Show login page on error
                if (LoginFrame != null)
                {
                    _ = LoginFrame.Navigate(typeof(ConnectionPage));

                    // Listen for the login event
                    if (LoginFrame.Content is ConnectionPage connectionPage)
                    {
                        // Remove any existing handlers to avoid duplicates
                        connectionPage.LoginSuccessful -= OnLoginSuccessful;
                        connectionPage.LoginSuccessful += OnLoginSuccessful;
                        Debug.WriteLine("MainWindow: Subscribed to LoginSuccessful event on error path");
                    }
                }
            }
        }

        private void SetWindowIcon()
        {
            try
            {
                // Get the HWND of the window
                nint hwnd = WindowNative.GetWindowHandle(this);

                // Get the AppWindow
                WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

                // Set the icon
                string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "PRTGLogo.png");
                Debug.WriteLine($"Trying to load icon from: {iconPath}");

                if (File.Exists(iconPath))
                {
                    appWindow.SetIcon(iconPath);
                    Debug.WriteLine($"Window icon set successfully: {iconPath}");
                }
                else
                {
                    // Try an alternative path
                    iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PRTGLogo.png");
                    Debug.WriteLine($"Trying alternative path: {iconPath}");

                    if (File.Exists(iconPath))
                    {
                        appWindow.SetIcon(iconPath);
                        Debug.WriteLine($"Window icon set successfully from alternative path: {iconPath}");
                    }
                    else
                    {
                        Debug.WriteLine($"Icon file not found at either path");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting window icon: {ex.Message}");
                Debug.WriteLine($"Exception details: {ex}");
            }
        }

        public void HandleAutoLogin(ConnectionInfo connectionInfo)
        {
            Debug.WriteLine("HandleAutoLogin called");
            _currentConnectionInfo = connectionInfo; // Store the connection info
            ConnectionManager.DebugConnectionInfo();

            try
            {
                // Make sure we're on the UI thread
                DispatcherQueue.TryEnqueue(() =>
                {
                try
                {
                    // Hide the login frame and show navigation
                    LoginFrame.Visibility = Visibility.Collapsed;
                    NavView.Visibility = Visibility.Visible;

                    // Navigate to the dashboard
                    _ = ContentFrame.Navigate(typeof(DashboardPage));
                    NavView.SelectedItem = NavView.MenuItems[0]; // Select dashboard

                    Debug.WriteLine("MainWindow: Navigation to dashboard complete in HandleAutoLogin");
                }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"MainWindow: Error in HandleAutoLogin UI operation: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow: Error in HandleAutoLogin: {ex.Message}");
            }
        }

        private void OnLoginSuccessful(object sender, ConnectionInfo connectionInfo)
        {
            Debug.WriteLine("MainWindow: OnLoginSuccessful called");
            _currentConnectionInfo = connectionInfo; // Store the connection info locally
            ConnectionManager.DebugConnectionInfo();

            try
            {
                // Make sure we're on the UI thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        // Hide the login frame and show navigation
                        LoginFrame.Visibility = Visibility.Collapsed;
                        NavView.Visibility = Visibility.Visible;

                        // Navigate to the dashboard
                        _ = ContentFrame.Navigate(typeof(DashboardPage));
                        NavView.SelectedItem = NavView.MenuItems[0]; // Select dashboard

                        Debug.WriteLine("MainWindow: Navigation to dashboard complete in OnLoginSuccessful");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"MainWindow: Error in OnLoginSuccessful UI operation: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow: Error in OnLoginSuccessful: {ex.Message}");
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            ArgumentNullException.ThrowIfNull(sender);

            if (args.IsSettingsSelected)
            {
                _ = ContentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            if (args.SelectedItemContainer is NavigationViewItem selectedItem)
            {
                string tag = selectedItem.Tag?.ToString();

                // Check if we have valid connection info
                if (!ConnectionManager.IsConnected)
                {
                    Debug.WriteLine("No connection info available for navigation");
                    ConnectionManager.DebugConnectionInfo();

                    // Show an error message
                    ShowConnectionErrorDialog();
                    return;
                }

                Debug.WriteLine($"Navigating to {tag}");
                ConnectionManager.DebugConnectionInfo();

                switch (tag)
                {
                    case "dashboard":
                        _ = ContentFrame.Navigate(typeof(DashboardPage));
                        break;
                    case "sensors":
                        _ = ContentFrame.Navigate(typeof(SensorsPage));
                        break;
                    case "devices":
                        _ = ContentFrame.Navigate(typeof(DevicesPage));
                        break;
                    case "reports":
                        _ = ContentFrame.Navigate(typeof(ReportsPage));
                        break;
                    case "settings":
                        _ = ContentFrame.Navigate(typeof(SettingsPage));
                        break;
                    case "logout":
                        Logout();
                        break;
                }
            }
        }

        private async void ShowConnectionErrorDialog()
        {
            ContentDialog dialog = new()
            {
                Title = "Connection Error",
                Content = "No connection information available. Please connect to a PRTG server.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            _ = await dialog.ShowAsync();

            // Navigate back to the connection page
            LoginFrame.Visibility = Visibility.Visible;
            NavView.Visibility = Visibility.Collapsed;
            _ = LoginFrame.Navigate(typeof(ConnectionPage));

            // Listen for the login event
            if (LoginFrame.Content is ConnectionPage connectionPage)
            {
                connectionPage.LoginSuccessful -= OnLoginSuccessful;
                connectionPage.LoginSuccessful += OnLoginSuccessful;
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(args);

            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(sender);

            Debug.WriteLine($"Navigation failed: {e.Exception.Message}");

            // Show an error message
            ContentDialog dialog = new()
            {
                Title = "Navigation Failed",
                Content = $"Failed to navigate to {e.SourcePageType.FullName}: {e.Exception.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private async void Logout()
        {
            // Confirm logout
            ContentDialog dialog = new()
            {
                Title = "Logout",
                Content = "Are you sure you want to logout?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Debug.WriteLine("Logging out and clearing connection info");

                // Remove connection info
                ConnectionManager.ClearConnection();
                _currentConnectionInfo = null;

                // Hide navigation and show login frame
                NavView.Visibility = Visibility.Collapsed;
                LoginFrame.Visibility = Visibility.Visible;

                // Navigate to login page
                _ = LoginFrame.Navigate(typeof(ConnectionPage));

                // Listen for login event
                if (LoginFrame.Content is ConnectionPage connectionPage)
                {
                    connectionPage.LoginSuccessful -= OnLoginSuccessful;
                    connectionPage.LoginSuccessful += OnLoginSuccessful;
                }
            }
        }
    }
}
