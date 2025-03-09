using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PRTGInsight.Services;
using PRTGInsight.Views;
using System;

namespace PRTGInsight
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if we have connection info
            if (ConnectionManager.CurrentConnection != null &&
                !string.IsNullOrEmpty(ConnectionManager.CurrentConnection.ServerUrl))
            {
                // Update the connection info in the UI
                UpdateConnectionInfo();

                // Navigate to the dashboard by default
                NavView.SelectedItem = NavView.MenuItems[0];
                ContentFrame.Navigate(typeof(DashboardPage));
            }
            else
            {
                // Navigate to the connection page if no connection info exists
                ContentFrame.Navigate(typeof(ConnectionPage));
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            NavigationViewItem selectedItem = args.SelectedItem as NavigationViewItem;
            if (selectedItem != null)
            {
                string tag = selectedItem.Tag.ToString();
                switch (tag)
                {
                    case "dashboard":
                        ContentFrame.Navigate(typeof(DashboardPage));
                        break;
                    case "devices":
                        ContentFrame.Navigate(typeof(DevicesPage));
                        break;
                    case "sensors":
                        ContentFrame.Navigate(typeof(SensorsPage));
                        break;
                    case "alerts":
                        ContentFrame.Navigate(typeof(AlertsPage));
                        break;
                    case "exports":
                        ContentFrame.Navigate(typeof(ExportsPage));
                        break;
                    case "refresh":
                        RefreshCurrentPage();
                        break;
                }
            }
        }

        private void RefreshCurrentPage()
        {
            // Show loading overlay
            LoadingOverlay.Visibility = Visibility.Visible;

            // Get the current page type
            Type currentPageType = ContentFrame.CurrentSourcePageType;

            // Refresh by re-navigating to the same page
            ContentFrame.Navigate(currentPageType);

            // Hide loading overlay after a short delay
            DispatcherQueue.TryEnqueue(async () => {
                await System.Threading.Tasks.Task.Delay(1000);
                LoadingOverlay.Visibility = Visibility.Collapsed;
            });
        }

        public void UpdateConnectionInfo()
        {
            if (ConnectionManager.CurrentConnection != null)
            {
                ServerInfoText.Text = $"Server: {ConnectionManager.CurrentConnection.ServerUrl}";
                ConnectionStatusText.Text = "Connected";
                ConnectionStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                ServerInfoText.Text = "Server: Not connected";
                ConnectionStatusText.Text = "Disconnected";
                ConnectionStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        // Method to handle navigation from ConnectionPage after successful login
        public void NavigateToDashboard()
        {
            UpdateConnectionInfo();
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(DashboardPage));
        }
    }
}
