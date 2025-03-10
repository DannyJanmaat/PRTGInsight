#define DISABLE_XAML_GENERATED_MAIN

using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace PRTGInsight
{
    public class Program
    {
        private static App _appInstance;

        [STAThread]
        private static void Main(string[] args)
        {
            // Set up enhanced exception handling
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) => {
                if (e.Exception is AccessViolationException)
                {
                    Debug.WriteLine($"FIRST CHANCE AccessViolationException: {e.Exception.Message}");
                }
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Debug.WriteLine($"CRITICAL: Unhandled exception: {e.ExceptionObject}");

                try
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        File.WriteAllText("crash_unhandled.txt",
                            $"Unhandled Exception: {DateTime.Now}\n{ex}\n\nStackTrace: {ex.StackTrace}");
                    }
                }
                catch { }
            };

            try
            {
                Debug.WriteLine("Application starting - initializing COM wrappers");
                WinRT.ComWrappersSupport.InitializeComWrappers();

                Debug.WriteLine("Application starting - calling Application.Start");

                // Create a simple app without icon loading
                Application.Start((p) =>
                {
                    try
                    {
                        Debug.WriteLine("Setting up dispatcher queue synchronization context");
                        var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                        var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                        System.Threading.SynchronizationContext.SetSynchronizationContext(context);

                        Debug.WriteLine("Creating App instance");
                        _appInstance = new App(); // Store in static field to prevent GC
                        GC.KeepAlive(_appInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in application startup: {ex}");
                        try
                        {
                            File.WriteAllText("crash_startup.txt",
                                $"Startup Exception: {DateTime.Now}\n{ex}\n\nStackTrace: {ex.StackTrace}");
                        }
                        catch { }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Critical error in Main: {ex}");
                try
                {
                    File.WriteAllText("crash_main.txt",
                        $"Main Exception: {DateTime.Now}\n{ex}\n\nStackTrace: {ex.StackTrace}");
                }
                catch { }
            }
        }
    }
}