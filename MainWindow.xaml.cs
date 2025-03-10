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
        private readonly Frame _contentFrame;
        public static new MainWindow Current { get; private set; }

        public MainWindow()
        {
            this.InitializeComponent();
            Title = "PRTG Insight";
            Current = this;

            // IMPORTANT: Remove icon setting from constructor
            // SetWindowIcon(); // Comment out or remove this line

            if (LoginFrame == null)
            {
                throw new InvalidOperationException("LoginFrame is not initialized.");
            }

            _contentFrame = new Frame();
            _contentFrame.NavigationFailed += ContentFrame_NavigationFailed;
            NavView.Content = _contentFrame;
            NavView.BackRequested += NavView_BackRequested;

            // Always show the authentication page first
            LoadConnectionInfoAsync();

            // Delayed style application to avoid memory issues during initialization
            _ = DispatcherQueue.TryEnqueue(CreateAndApplyStyles);
        }

        private void CreateAndApplyStyles()
        {
            try
            {
                // Create ListViewItemStyle programmatically
                Style listViewItemStyle = new(typeof(ListViewItem));
                listViewItemStyle.Setters.Add(new Setter(ListViewItem.MinHeightProperty, 40.0));
                listViewItemStyle.Setters.Add(new Setter(ListViewItem.PaddingProperty, new Thickness(12, 6, 12, 6)));
                listViewItemStyle.Setters.Add(new Setter(ListViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
                listViewItemStyle.Setters.Add(new Setter(ListViewItem.VerticalContentAlignmentProperty, VerticalAlignment.Center));

                // Add style to application resources
                Application.Current.Resources["ListViewItemStyle"] = listViewItemStyle;

                Debug.WriteLine("Custom styles applied programmatically");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating custom styles: {ex.Message}");
            }
        }

        private void LoadConnectionInfoAsync()
        {
            try
            {
                // Load connection info from file-based storage
                ConnectionInfo connectionInfo = SettingsService.LoadConnectionInfo();

                // Even if saved info exists, we want the user to authenticate each time.
                if (connectionInfo != null && !string.IsNullOrEmpty(connectionInfo.ServerUrl))
                {
                    ConnectionManager.CurrentConnection = connectionInfo;
                    Debug.WriteLine("MainWindow: Saved connection info loaded but not auto-logging in.");
                    ConnectionManager.DebugConnectionInfo();
                }
                else
                {
                    Debug.WriteLine("MainWindow: No saved connection info found.");
                }

                // Always show the login page
                if (LoginFrame != null)
                {
                    _ = LoginFrame.Navigate(typeof(ConnectionPage));
                    if (LoginFrame.Content is ConnectionPage connectionPage)
                    {
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
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow: Error loading connection info: {ex.Message}");
                if (LoginFrame != null)
                {
                    _ = LoginFrame.Navigate(typeof(ConnectionPage));
                    if (LoginFrame.Content is ConnectionPage connectionPage)
                    {
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
                // Get the window handle and ensure it's valid.
                IntPtr hWnd = WindowNative.GetWindowHandle(this);
                if (hWnd == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid window handle received.");
                    return;
                }

                // Get the WindowId.
                WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                // No null check is needed here because WindowId is a struct.

                // Get the AppWindow and ensure it's not null.
                AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
                if (appWindow == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to obtain AppWindow.");
                    return;
                }

                // Simplified array initialization
                string[] potentialIconPaths =
                [
                    Path.Combine(AppContext.BaseDirectory, "Assets", "PRTGInsight.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PRTGInsight.ico"),
                    Path.Combine(AppContext.BaseDirectory, "Assets", "PRTGLogo.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PRTGLogo.png")
                ];

                bool iconSet = false;
                foreach (string iconPath in potentialIconPaths)
                {
                    if (File.Exists(iconPath))
                    {
                        try
                        {
                            appWindow.SetIcon(iconPath);
                            System.Diagnostics.Debug.WriteLine($"Set window icon from: {iconPath}");
                            iconSet = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to set icon from {iconPath}: {ex.Message}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Icon file not found: {iconPath}");
                    }
                }

                if (!iconSet)
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Could not set window icon from any source");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting window icon: {ex.Message}");
            }
        }

        public void HandleAutoLogin(ConnectionInfo connectionInfo)
        {
            Debug.WriteLine("HandleAutoLogin called");
            _currentConnectionInfo = connectionInfo;
            ConnectionManager.DebugConnectionInfo();

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    LoginFrame.Visibility = Visibility.Collapsed;
                    NavView.Visibility = Visibility.Visible;
                    NavView.IsPaneOpen = true;
                    _ = _contentFrame.Navigate(typeof(DashboardPage));
                    NavView.SelectedItem = NavView.MenuItems[0];
                    Debug.WriteLine($"MainWindow: Navigation to dashboard complete, NavView visibility: {NavView.Visibility}, IsPaneOpen: {NavView.IsPaneOpen}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MainWindow: Error in HandleAutoLogin UI operation: {ex.Message}");
                }
            });
        }

        private void OnLoginSuccessful(object sender, ConnectionInfo connectionInfo)
        {
            Debug.WriteLine("MainWindow: OnLoginSuccessful called");
            _currentConnectionInfo = connectionInfo;
            ConnectionManager.DebugConnectionInfo();

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    // Make sure the NavigationView is visible
                    LoginFrame.Visibility = Visibility.Collapsed;
                    NavView.Visibility = Visibility.Visible;

                    // Explicitly open the pane
                    NavView.IsPaneOpen = true;

                    // Navigate to dashboard
                    _ = _contentFrame.Navigate(typeof(DashboardPage));
                    NavView.SelectedItem = NavView.MenuItems[0];

                    Debug.WriteLine($"MainWindow: Navigation to dashboard complete, NavView visibility: {NavView.Visibility}, IsPaneOpen: {NavView.IsPaneOpen}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MainWindow: Error in OnLoginSuccessful UI operation: {ex.Message}");
                }
            });
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            ArgumentNullException.ThrowIfNull(sender);

            if (args.IsSettingsSelected)
            {
                _ = _contentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            if (args.SelectedItemContainer is NavigationViewItem selectedItem)
            {
                string tag = selectedItem.Tag?.ToString();
                if (!ConnectionManager.IsConnected)
                {
                    Debug.WriteLine("No connection info available for navigation");
                    ConnectionManager.DebugConnectionInfo();
                    ShowConnectionErrorDialog();
                    return;
                }

                Debug.WriteLine($"Navigating to {tag}");
                ConnectionManager.DebugConnectionInfo();

                switch (tag)
                {
                    case "dashboard":
                        _ = _contentFrame.Navigate(typeof(DashboardPage));
                        break;
                    case "sensors":
                        _ = _contentFrame.Navigate(typeof(SensorsPage));
                        break;
                    case "devices":
                        _ = _contentFrame.Navigate(typeof(DevicesPage));
                        break;
                    case "alerts":
                        _ = _contentFrame.Navigate(typeof(AlertsPage));
                        break;
                    case "exports":
                        _ = _contentFrame.Navigate(typeof(ExportsPage));
                        break;
                    case "reports":
                        _ = _contentFrame.Navigate(typeof(ReportsPage));
                        break;
                    case "settings":
                        _ = _contentFrame.Navigate(typeof(SettingsPage));
                        break;
                    case "logout":
                        Logout();
                        break;
                }
            }
        }

        private void RefreshCurrentPage()
        {
            try
            {
                Type currentPageType = _contentFrame.CurrentSourcePageType;
                if (currentPageType == null)
                {
                    return;
                }

                if (_contentFrame.Content is FrameworkElement contentElement && contentElement.FindName("LoadingOverlay") is Grid loadingOverlay)
                {
                    loadingOverlay.Visibility = Visibility.Visible;
                }

                if (_contentFrame.Content is FrameworkElement page)
                {
                    System.Reflection.MethodInfo refreshMethod = page.GetType().GetMethod("RefreshData");
                    if (refreshMethod != null)
                    {
                        _ = refreshMethod.Invoke(page, null);
                        Debug.WriteLine($"Called RefreshData on {currentPageType.Name}");
                    }
                    else
                    {
                        _ = _contentFrame.Navigate(currentPageType);
                        Debug.WriteLine($"Re-navigated to {currentPageType.Name} to refresh");
                    }
                }

                if (_contentFrame.Content is FrameworkElement contentWithOverlay && contentWithOverlay.FindName("LoadingOverlay") is Grid overlay)
                {
                    _ = DispatcherQueue.TryEnqueue(async () =>
                    {
                        await Task.Delay(1000);
                        overlay.Visibility = Visibility.Collapsed;
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing current page: {ex.Message}");
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

            LoginFrame.Visibility = Visibility.Visible;
            NavView.Visibility = Visibility.Collapsed;
            _ = LoginFrame.Navigate(typeof(ConnectionPage));

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

            if (_contentFrame.CanGoBack)
            {
                _contentFrame.GoBack();
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(sender);
            Debug.WriteLine($"Navigation failed: {e.Exception.Message}");
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
                ConnectionManager.ClearConnection();
                _currentConnectionInfo = null;
                NavView.Visibility = Visibility.Collapsed;
                LoginFrame.Visibility = Visibility.Visible;
                _ = LoginFrame.Navigate(typeof(ConnectionPage));

                if (LoginFrame.Content is ConnectionPage connectionPage)
                {
                    connectionPage.LoginSuccessful -= OnLoginSuccessful;
                    connectionPage.LoginSuccessful += OnLoginSuccessful;
                }
            }
        }
    }
}