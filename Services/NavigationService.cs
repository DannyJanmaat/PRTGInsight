// Services/NavigationService.cs
using Microsoft.UI.Xaml.Controls;
using System;

namespace PRTGInsight.Services
{
    public class NavigationService(Frame _frame)
    {
        public bool Navigate(Type pageType, object parameter = null)
        {
            return _frame.Navigate(pageType, parameter);
        }

        public void GoBack()
        {
            if (_frame.CanGoBack)
            {
                _frame.GoBack();
            }
        }
    }
}
