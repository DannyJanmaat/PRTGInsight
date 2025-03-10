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
                _window = new MainWindow();
                MainWindow = _window;

                // Activate window
                _window.Activate();

                // Set window title
                _window.Title = "PRTG Insight";

                LoadAdditionalResources();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Critical error in OnLaunched: {ex}");
            }
        }

        private void LoadAdditionalResources()
        {
            try
            {
                // Create and load the resource dictionary
                ResourceDictionary stylesDictionary = [];

                // Load the resource dictionary - The URI format is important
                Uri resourceUri = new("ms-appx:///Styles.xaml", UriKind.Absolute);
                stylesDictionary.Source = resourceUri;

                // Add to the application resources
                Resources.MergedDictionaries.Add(stylesDictionary);

                Debug.WriteLine("Additional resources loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading additional resources: {ex.Message}");
                // Continue execution even if styles fail to load
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine($"Navigation failed: {e.SourcePageType.FullName} - {e.Exception?.Message}");
            // Log but don't throw to avoid crashing the app
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}