using PRTGInsight.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PRTGInsight.Services.Prtg
{
    public class PrtgService
    {
        #region Connection Methods

        public static Task<PrtgStatus> TestConnectionAsync(string serverUrl, string username, string password, CancellationToken cancellationToken)
        {
            return PrtgConnectionService.TestConnectionAsync(serverUrl, username, password, cancellationToken);
        }

        public static Task<PrtgStatus> TestConnectionWithApiKeyAsync(string serverUrl, string apiKey, CancellationToken cancellationToken)
        {
            return PrtgConnectionService.TestConnectionWithApiKeyAsync(serverUrl, apiKey, cancellationToken);
        }

        public static Task<PrtgSensorStatus> GetSensorStatusAsync(ConnectionInfo connectionInfo, CancellationToken _)
        {
            return PrtgDashboardService.GetSensorStatusAsync(connectionInfo);
        }

        public static Task<PrtgSystemStatus> GetSystemStatusAsync(ConnectionInfo connectionInfo, CancellationToken _ = default)
        {
            return PrtgDashboardService.GetSystemStatusAsync(connectionInfo);
        }

        public static Task<List<PrtgAlert>> GetRecentAlertsAsync(ConnectionInfo connectionInfo, CancellationToken _ = default)
        {
            return PrtgDashboardService.GetRecentAlertsAsync(connectionInfo);
        }

        #endregion

        #region Sensor Methods

        public static Task<List<PrtgSensor>> GetSensorsAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            return PrtgSensorService.GetSensorsAsync(connectionInfo, cancellationToken);
        }

        public static Task<PrtgSensorDetails> GetSensorDetailsAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken cancellationToken = default)
        {
            return PrtgSensorService.GetSensorDetailsAsync(connectionInfo, sensorId, cancellationToken);
        }

        public static Task<List<PrtgSensorHistoryItem>> GetSensorHistoryAsync(
            ConnectionInfo connectionInfo,
            int sensorId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return PrtgSensorService.GetSensorHistoryAsync(connectionInfo, sensorId, startDate, endDate, cancellationToken);
        }

        public static Task<string> GetSensorGraphUrlAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken cancellationToken = default)
        {
            return PrtgSensorService.GetSensorGraphUrlAsync(connectionInfo, sensorId, cancellationToken);
        }

        public static Task<bool> ScanSensorNowAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken cancellationToken = default)
        {
            return PrtgSensorService.ScanSensorNowAsync(connectionInfo, sensorId, cancellationToken);
        }

        public static Task<bool> PauseSensorAsync(ConnectionInfo connectionInfo, int sensorId, string pauseMessage = "Paused by PRTG Insight", CancellationToken cancellationToken = default)
        {
            return PrtgSensorService.PauseSensorAsync(connectionInfo, sensorId, pauseMessage, cancellationToken);
        }

        public static Task<bool> ResumeSensorAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken cancellationToken = default)
        {
            return PrtgSensorService.ResumeSensorAsync(connectionInfo, sensorId, cancellationToken);
        }

        #endregion

        #region Device Methods

        public static Task<List<PrtgDevice>> GetDevicesAsync(string serverUrl, string apiKey, CancellationToken cancellationToken)
        {
            return PrtgDeviceService.GetDevicesAsync(serverUrl, apiKey, cancellationToken);
        }

        public static Task<List<PrtgDevice>> GetDevicesWithCredentialsAsync(string serverUrl, string username, string password, CancellationToken cancellationToken)
        {
            return PrtgDeviceService.GetDevicesWithCredentialsAsync(serverUrl, username, password, cancellationToken);
        }

        public static Task<List<PrtgDevice>> GetDevicesAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            return PrtgDeviceService.GetDevicesAsync(connectionInfo, cancellationToken);
        }

        #endregion

        #region Compatibility Methods for Existing Code

        // These methods are added for backward compatibility with existing code
        public static Task<List<PrtgSensor>> GetSensorsWithCredentialsAsync(string serverUrl, string username, string password, CancellationToken cancellationToken)
        {
            ConnectionInfo connectionInfo = new()
            {
                ServerUrl = serverUrl,
                Username = username,
                Password = password,
                UseApiKey = false
            };

            return PrtgSensorService.GetSensorsAsync(connectionInfo, cancellationToken);
        }

        #endregion
    }
}
