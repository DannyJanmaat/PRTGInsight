using PRTGInsight.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PRTGInsight.Services
{
    public static class ConnectionManager
    {
        private static ConnectionInfo _currentConnection;

        public static ConnectionInfo CurrentConnection
        {
            get
            {
                if (_currentConnection == null)
                {
                    Debug.WriteLine("ConnectionManager: CurrentConnection is null, trying to load from settings");
                    LoadFromSettingsSync();
                }
                return _currentConnection;
            }
            set
            {
                _currentConnection = value;
                Debug.WriteLine($"ConnectionManager: Connection set to {value?.ServerUrl ?? "null"}");

                // Save the connection info to settings
                if (value != null)
                {
                    _ = SaveConnectionInfoAsync(value);
                }
            }
        }

        // Synchronous version of LoadFromSettings for use in property getter
        private static void LoadFromSettingsSync()
        {
            try
            {
                ConnectionInfo connectionInfo = SettingsService.LoadConnectionInfo();
                if (connectionInfo != null && !string.IsNullOrEmpty(connectionInfo.ServerUrl))
                {
                    _currentConnection = connectionInfo;
                    Debug.WriteLine($"ConnectionManager: Loaded connection from settings: {connectionInfo.ServerUrl}");
                }
                else
                {
                    Debug.WriteLine("ConnectionManager: No connection found in settings");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConnectionManager: Error loading connection from settings: {ex.Message}");
            }
        }

        public static void DebugConnectionInfo()
        {
            if (_currentConnection != null)
            {
                Debug.WriteLine($"CONNECTION INFO DEBUG:");
                Debug.WriteLine($"  ServerUrl: {_currentConnection.ServerUrl}");
                Debug.WriteLine($"  UseApiKey: {_currentConnection.UseApiKey}");
                Debug.WriteLine($"  ApiKey: {(_currentConnection.ApiKey != null ? "Present" : "Null")}");
                Debug.WriteLine($"  Username: {_currentConnection.Username}");
                Debug.WriteLine($"  Password: {(_currentConnection.Password != null ? "Present" : "Null")}");
                Debug.WriteLine($"  PrtgVersion: {_currentConnection.PrtgVersion}");
            }
            else
            {
                Debug.WriteLine("CONNECTION INFO DEBUG: No connection info available");
            }
        }

        public static bool IsConnected => _currentConnection != null && !string.IsNullOrEmpty(_currentConnection.ServerUrl);

        public static void ClearConnection()
        {
            _currentConnection = null;
            Debug.WriteLine("ConnectionManager: Connection cleared");
            SettingsService.ClearConnectionInfo();
        }

        public static async Task LoadFromSettingsAsync()
        {
            try
            {
                ConnectionInfo connectionInfo = await SettingsService.LoadConnectionInfoAsync();
                if (connectionInfo != null && !string.IsNullOrEmpty(connectionInfo.ServerUrl))
                {
                    _currentConnection = connectionInfo;
                    Debug.WriteLine($"ConnectionManager: Loaded connection from settings: {connectionInfo.ServerUrl}");
                }
                else
                {
                    Debug.WriteLine("ConnectionManager: No connection found in settings");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConnectionManager: Error loading connection from settings: {ex.Message}");
            }
        }

        private static async Task SaveConnectionInfoAsync(ConnectionInfo connectionInfo)
        {
            ArgumentNullException.ThrowIfNull(connectionInfo);

            try
            {
                await SettingsService.SaveConnectionInfoAsync(connectionInfo);
                Debug.WriteLine("ConnectionManager: Connection info saved to settings");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConnectionManager: Error saving connection info: {ex.Message}");
            }
        }
    }
}
