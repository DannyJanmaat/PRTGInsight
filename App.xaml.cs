using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Helpers;
using PRTGInsight.Services;
using PRTGInsight.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using WinRT.Interop;

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
            try
            {
                _window = new Window();
                MainWindow = _window;

                // Use a local variable for the frame to ensure it doesn't get collected
                Frame rootFrame = new();
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Set window content
                _window.Content = rootFrame;
                _window.Title = "PRTG Insight"; // Set title directly

                // IMPORTANT: No icon setting at all during startup

                // Activate window first before any further operations
                _window.Activate();

                // Now that the window is activated, navigate directly to the connection page
                _window.DispatcherQueue.TryEnqueue(() => {
                    // Ensure window is fully initialized
                    if (rootFrame.Content == null)
                    {
                        try
                        {
                            rootFrame.Navigate(typeof(ConnectionPage), args.Arguments);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Critical error in OnLaunched: {ex}");
            }
        }


        private void DelayedSetWindowIcon()
        {
            try
            {
                // Get the window handle and ensure it's valid.
                IntPtr hWnd = WindowNative.GetWindowHandle(_window);
                if (hWnd == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid window handle received.");
                    return;
                }

                // Get the WindowId and AppWindow.
                WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
                if (appWindow == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to obtain AppWindow.");
                    return;
                }

                // Use proper array initialization syntax.
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
                            // If a particular icon file causes an exception (for example, if the file is corrupt),
                            // you might opt to continue to the next available file.
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
                        if (MainWindow.Content is Frame rootFrame)
                        {
                            _ = rootFrame.Navigate(typeof(ConnectionPage));
                        }
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
