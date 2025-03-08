using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace PRTGInsight.Views
{
    public sealed partial class SettingsPage : Page
    {
        private ConnectionInfo _connectionInfo;

        public SettingsPage()
        {
            this.InitializeComponent();

            // Voeg event handlers toe via code in plaats van XAML
            if (RefreshIntervalBox != null)
                RefreshIntervalBox.ValueChanged += RefreshIntervalBox_ValueChanged;

            // Andere initialisatie
            this.Loaded += OnPageLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ConnectionInfo connectionInfo)
            {
                _connectionInfo = connectionInfo;
                UpdateUI();
            }
            else
            {
                // Probeer de verbindingsgegevens te laden als ze niet zijn doorgegeven
                _ = LoadConnectionInfoAsync();
            }
        }

        private async Task LoadConnectionInfoAsync()
        {
            try
            {
                _connectionInfo = SettingsService.LoadConnectionInfo();

                if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
                {
                    // Toon een foutmelding
                    ContentDialog dialog = new()
                    {
                        Title = "Connection Error",
                        Content = "No connection information available. Please connect to a PRTG server.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();

                    // Navigeer terug naar de verbindingspagina
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                    }
                    else
                    {
                        Frame.Navigate(typeof(ConnectionPage));
                    }

                    return;
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                // Toon een foutmelding
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to load connection information: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void OnPageLoaded(object _, RoutedEventArgs __)
        {
            // Controleer of we verbindingsgegevens hebben
            if (_connectionInfo == null || string.IsNullOrEmpty(_connectionInfo.ServerUrl))
            {
                await LoadConnectionInfoAsync();
            }

            // Laad het opgeslagen thema
            await LoadThemePreferenceAsync();
        }

        private async Task LoadThemePreferenceAsync()
        {
            try
            {
                // Laad de thema-instelling uit de lokale instellingen
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings.Values.TryGetValue("AppTheme", out object value))
                {
                    string themeString = value as string;
                    if (!string.IsNullOrEmpty(themeString) && Enum.TryParse<ElementTheme>(themeString, out var theme))
                    {
                        // Pas het thema toe
                        this.RequestedTheme = theme;

                        // Update de toggle
                        if (ThemeToggle != null)
                        {
                            ThemeToggle.IsOn = theme == ElementTheme.Dark;
                        }

                        // Probeer het thema toe te passen op het hoofdvenster
                        try
                        {
                            if (Window.Current.Content is FrameworkElement rootElement)
                            {
                                rootElement.RequestedTheme = theme;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error setting theme: {ex.Message}");
                        }
                    }
                }

                await Task.CompletedTask; // Alleen om de methode async te maken
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading theme preference: {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            if (_connectionInfo != null && ServerUrlTextBlock != null && AuthTypeTextBlock != null && VersionTextBlock != null)
            {
                ServerUrlTextBlock.Text = $"Server URL: {_connectionInfo.ServerUrl}";
                AuthTypeTextBlock.Text = $"Authentication: {(_connectionInfo.UseApiKey ? "API Key" : "Username/Password")}";
                VersionTextBlock.Text = $"PRTG Version: {_connectionInfo.PrtgVersion}";
            }
        }

        private void ReconnectButton_Click(object _, RoutedEventArgs __)
        {
            // Navigate to connection page
            Frame.Navigate(typeof(ConnectionPage));
        }

        private void ThemeToggle_Toggled(object _, RoutedEventArgs __)
        {
            // Implement theme switching
            if (ThemeToggle != null)
            {
                var requestedTheme = ThemeToggle.IsOn ? ElementTheme.Dark : ElementTheme.Light;

                // Pas het thema toe op de huidige pagina
                this.RequestedTheme = requestedTheme;

                // Sla de thema-instelling op voor toekomstig gebruik
                _ = SaveThemePreferenceAsync(requestedTheme);

                // Probeer het thema toe te passen op het hoofdvenster
                try
                {
                    if (Window.Current.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = requestedTheme;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting theme: {ex.Message}");
                }
            }
        }

        private static async Task SaveThemePreferenceAsync(ElementTheme theme)
        {
            try
            {
                // Sla de thema-instelling op in de lokale instellingen
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["AppTheme"] = theme.ToString();

                await Task.CompletedTask; // Alleen om de methode async te maken
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme preference: {ex.Message}");
            }
        }

        private void AutoRefreshToggle_Toggled(object _, RoutedEventArgs __)
        {
            // Enable/disable refresh interval box based on toggle state
            if (RefreshIntervalBox != null && AutoRefreshToggle != null)
            {
                RefreshIntervalBox.IsEnabled = AutoRefreshToggle.IsOn;
            }
        }

        private void RefreshIntervalBox_ValueChanged(NumberBox _, NumberBoxValueChangedEventArgs __)
        {
            // In a real app, you would save this setting and apply it to your app
        }
    }
}
