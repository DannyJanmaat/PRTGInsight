using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Helpers;
using PRTGInsight.Services;
using PRTGInsight.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;

namespace PRTGInsight
{
    public partial class App : Application
    {
        private Window _window;
        public static Window MainWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
            CoreApplication.Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new Window();
            MainWindow = _window; // Set the static reference
            WindowHelper.Initialize(_window); // Initialize our helper

            Frame rootFrame = new();
            rootFrame.NavigationFailed += OnNavigationFailed;
            _window.Content = rootFrame;

            if (rootFrame.Content == null)
            {
                _ = rootFrame.Navigate(typeof(MainPage), args.Arguments);
            }

            _window.Activate();

            // Ensure connection information is loaded after the main window is created
            _ = Task.Run(async () =>
            {
                // Add a small delay to ensure window is fully initialized
                await Task.Delay(500);
                await LoadConnectionInfoAsync();
            });
        }

        private static async Task LoadConnectionInfoAsync()
        {
            try
            {
                Models.ConnectionInfo connectionInfo = SettingsService.LoadConnectionInfo();

                // Dispatch to UI thread but don't assign the result (which is void)
                await MainWindow.DispatcherQueue.EnqueueAsync(() =>
                {
                    if (connectionInfo == null || string.IsNullOrEmpty(connectionInfo.ServerUrl))
                    {
                        // Handle no connection info scenario
                        System.Diagnostics.Debug.WriteLine("No saved connection, showing login page");
                        Frame rootFrame = MainWindow.Content as Frame;
                        _ = rootFrame.Navigate(typeof(ConnectionPage));
                    }
                    else
                    {
                        // Set the connection info in the ConnectionManager
                        ConnectionManager.CurrentConnection = connectionInfo;
                        System.Diagnostics.Debug.WriteLine("Loaded connection info from settings");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading connection info: {ex.Message}");
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
