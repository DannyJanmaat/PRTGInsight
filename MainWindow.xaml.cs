using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using PRTGInsight.Views;
using System;
using System.Diagnostics;

namespace PRTGInsight
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            Title = "PRTG Insight";

            // Setup content frame navigation
            ContentFrame.NavigationFailed += ContentFrame_NavigationFailed;

            // Configure navigation view
            MainNavigationView.PaneOpening += MainNavigationView_PaneOpening;
            MainNavigationView.PaneClosing += MainNavigationView_PaneClosing;

            // Load connection info and prepare login
            LoadConnectionInfoAsync();
        }

        private void MainNavigationView_PaneOpening(NavigationView sender, object args)
        {
            // Optional: Add any specific behaviors when pane opens
            Debug.WriteLine("Navigation pane opening");
        }

        private void MainNavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            // Optional: Add any specific behaviors when pane closes
            Debug.WriteLine("Navigation pane closing");
        }

        private void LoadConnectionInfoAsync()
        {
            try
            {
                // Load connection info from storage
                ConnectionInfo connectionInfo = SettingsService.LoadConnectionInfo();

                // Always show login page
                if (LoginFrame != null)
                {
                    _ = LoginFrame.Navigate(typeof(ConnectionPage));

                    if (LoginFrame.Content is ConnectionPage connectionPage)
                    {
                        // Wire up login successful event
                        connectionPage.LoginSuccessful -= OnLoginSuccessful;
                        connectionPage.LoginSuccessful += OnLoginSuccessful;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading connection info: {ex.Message}");
            }
        }

        private void OnLoginSuccessful(object sender, ConnectionInfo connectionInfo)
        {
            Debug.WriteLine("Login successful, preparing navigation");

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    // Hide login frame
                    LoginFrame.Visibility = Visibility.Collapsed;

                    // Show and configure navigation view
                    MainNavigationView.Visibility = Visibility.Visible;

                    // Navigate to dashboard
                    _ = ContentFrame.Navigate(typeof(DashboardPage));

                    // Select first menu item (dashboard)
                    if (MainNavigationView.MenuItems.Count > 0)
                    {
                        MainNavigationView.SelectedItem = MainNavigationView.MenuItems[0];
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in login successful handler: {ex.Message}");
                }
            });
        }

        private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                _ = ContentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            if (args.SelectedItemContainer is NavigationViewItem selectedItem)
            {
                string tag = selectedItem.Tag?.ToString();

                // Ensure connection is valid
                if (!ConnectionManager.IsConnected && tag != "logout")
                {
                    ShowConnectionErrorDialog();
                    return;
                }

                // Navigation logic
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
                    case "alerts":
                        _ = ContentFrame.Navigate(typeof(AlertsPage));
                        break;
                    case "exports":
                        _ = ContentFrame.Navigate(typeof(ExportsPage));
                        break;
                    case "reports":
                        _ = ContentFrame.Navigate(typeof(ReportsPage));
                        break;
                    case "settings":
                        _ = ContentFrame.Navigate(typeof(SettingsPage));
                        break;
                    case "logout":
                        PerformLogout();
                        break;
                }
            }
        }

        private async void PerformLogout()
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
                // Clear connection info
                ConnectionManager.ClearConnection();

                // Reset UI
                MainNavigationView.Visibility = Visibility.Collapsed;
                LoginFrame.Visibility = Visibility.Visible;
                _ = LoginFrame.Navigate(typeof(ConnectionPage));
            }
        }

        private void ShowConnectionErrorDialog()
        {
            ContentDialog dialog = new()
            {
                Title = "Connection Error",
                Content = "No connection information available. Please connect to a PRTG server.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            _ = dialog.ShowAsync();

            // Reset to login page
            MainNavigationView.Visibility = Visibility.Collapsed;
            LoginFrame.Visibility = Visibility.Visible;
            _ = LoginFrame.Navigate(typeof(ConnectionPage));
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine($"Navigation failed: {e.Exception.Message}");

            ContentDialog dialog = new()
            {
                Title = "Navigation Failed",
                Content = $"Could not navigate to {e.SourcePageType.FullName}: {e.Exception.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            _ = dialog.ShowAsync();
        }
    }
}