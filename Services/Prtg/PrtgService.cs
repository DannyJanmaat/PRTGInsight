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

        public static Task<PrtgSensorStatus> GetSensorStatusAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            return PrtgDashboardService.GetSensorStatusAsync(connectionInfo, cancellationToken);
        }

        public static Task<PrtgSystemStatus> GetSystemStatusAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            return PrtgDashboardService.GetSystemStatusAsync(connectionInfo, cancellationToken);
        }

        public static Task<List<PrtgAlert>> GetRecentAlertsAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            return PrtgDashboardService.GetRecentAlertsAsync(connectionInfo, cancellationToken);
        }

        #endregion

        #region Sensor Methods

        // Instead of calling GetSensorsAsync (which is missing), use GetAllSensorsAsync.
        public static Task<List<PrtgSensor>> GetSensorsAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            return PrtgSensorService.GetAllSensorsAsync(connectionInfo, cancellationToken);
        }

        // Sensor details method is not available in PrtgSensorService so we stub it.
        public static Task<PrtgSensorDetails> GetSensorDetailsAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("GetSensorDetailsAsync is not implemented.");
        }

        // Sensor history method stub.
        public static Task<List<PrtgSensorHistoryItem>> GetSensorHistoryAsync(
            ConnectionInfo connectionInfo,
            int sensorId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("GetSensorHistoryAsync is not implemented.");
        }

        // Sensor graph URL method stub.
        public static Task<string> GetSensorGraphUrlAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("GetSensorGraphUrlAsync is not implemented.");
        }

        // Sensor scan now method stub.
        public static Task<bool> ScanSensorNowAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("ScanSensorNowAsync is not implemented.");
        }

        // Sensor pause method stub.
        public static Task<bool> PauseSensorAsync(ConnectionInfo connectionInfo, int sensorId, string pauseMessage = "Paused by PRTG Insight", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("PauseSensorAsync is not implemented.");
        }

        // Sensor resume method stub.
        public static Task<bool> ResumeSensorAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("ResumeSensorAsync is not implemented.");
        }

        #endregion

        #region Device Methods

        // For methods taking a serverUrl & apiKey, we compose a ConnectionInfo.
        public static Task<List<PrtgDevice>> GetDevicesAsync(string serverUrl, string apiKey, CancellationToken cancellationToken)
        {
            ConnectionInfo connectionInfo = new()
            {
                ServerUrl = serverUrl,
                ApiKey = apiKey,
                UseApiKey = true
            };

            return PrtgDeviceService.GetAllDevicesAsync(connectionInfo, cancellationToken);
        }

        public static Task<List<PrtgDevice>> GetDevicesWithCredentialsAsync(string serverUrl, string username, string password, CancellationToken cancellationToken)
        {
            ConnectionInfo connectionInfo = new()
            {
                ServerUrl = serverUrl,
                Username = username,
                Password = password,
                UseApiKey = false
            };

            return PrtgDeviceService.GetAllDevicesAsync(connectionInfo, cancellationToken);
        }

        public static Task<List<PrtgDevice>> GetDevicesAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            return PrtgDeviceService.GetAllDevicesAsync(connectionInfo, cancellationToken);
        }

        #endregion

        #region Compatibility Methods for Existing Code

        // These methods are added for backward compatibility with existing code.
        public static Task<List<PrtgSensor>> GetSensorsWithCredentialsAsync(string serverUrl, string username, string password, CancellationToken cancellationToken)
        {
            ConnectionInfo connectionInfo = new()
            {
                ServerUrl = serverUrl,
                Username = username,
                Password = password,
                UseApiKey = false
            };

            return PrtgSensorService.GetAllSensorsAsync(connectionInfo, cancellationToken);
        }

        #endregion
    }
}
