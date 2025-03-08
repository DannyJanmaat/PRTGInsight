using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using System;
using WinRT.Interop;
using Microsoft.UI;

namespace PRTGInsight.Helpers
{
    public static class WindowHelper
    {
        private static Window _mainWindow;

        public static Window MainWindow => _mainWindow;

        public static void Initialize(Window window)
        {
            _mainWindow = window;
        }

        public static IntPtr GetWindowHandle(Window window)
        {
            return WindowNative.GetWindowHandle(window);
        }

        public static AppWindow GetAppWindow(Window window)
        {
            var windowId = Win32Interop.GetWindowIdFromWindow(GetWindowHandle(window));
            return AppWindow.GetFromWindowId(windowId);
        }
    }
}
