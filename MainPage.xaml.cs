using Microsoft.UI.Xaml.Controls;
using PRTGInsight.Views;

namespace PRTGInsight
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Navigate to the connection page when MainPage is loaded
            MainFrame.Navigate(typeof(ConnectionPage));
        }
    }
}
