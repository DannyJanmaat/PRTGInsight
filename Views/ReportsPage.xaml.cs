using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PRTGInsight.Models;
using PRTGInsight.Services;
using System;
using System.Threading.Tasks;

namespace PRTGInsight.Views
{
    public sealed partial class ReportsPage : Page
    {
        private ConnectionInfo _connectionInfo;

        public ReportsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ConnectionInfo connectionInfo)
            {
                _connectionInfo = connectionInfo;
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
                }
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
    }
}
