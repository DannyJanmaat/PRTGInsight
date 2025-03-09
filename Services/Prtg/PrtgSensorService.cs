using PrtgAPI;
using PRTGInsight.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions; // Added for [GeneratedRegex] support
using System.Threading;
using System.Threading.Tasks;

namespace PRTGInsight.Services.Prtg
{
    public partial class PrtgSensorService : BasePrtgService
    {
        /// <summary>
        /// Gets all sensors from the PRTG server.
        /// </summary>
        /// <param name="connectionInfo">Connection information for the PRTG server</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of all sensors</returns>
        public static async Task<List<PrtgSensor>> GetAllSensorsAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine("Getting all sensors");

                // Use the helper method to get sensors
                var sensors = await GetSensorsAsync(connectionInfo, cancellationToken);

                Debug.WriteLine($"Retrieved {sensors.Count} sensors");
                return sensors;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting all sensors: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets sensors by status (up, down, warning, etc.)
        /// </summary>
        /// <param name="connectionInfo">Connection information for the PRTG server</param>
        /// <param name="status">Status to filter by (Up, Down, Warning, etc.)</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of filtered sensors</returns>
        public static async Task<List<PrtgSensor>> GetSensorsByStatusAsync(
            ConnectionInfo connectionInfo,
            string status,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine($"Getting sensors with status: {status}");

                // Get all sensors first
                var allSensors = await GetAllSensorsAsync(connectionInfo, cancellationToken);

                // Filter by status if specified
                if (!string.IsNullOrWhiteSpace(status))
                {
                    allSensors = [.. allSensors.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase))];
                }

                Debug.WriteLine($"Retrieved {allSensors.Count} sensors with status {status}");
                return allSensors;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting sensors by status: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets sensors for a specific device.
        /// </summary>
        /// <param name="connectionInfo">Connection information for the PRTG server</param>
        /// <param name="deviceId">The device ID to filter sensors by</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of filtered sensors</returns>
        public static async Task<List<PrtgSensor>> GetSensorsForDeviceAsync(
            ConnectionInfo connectionInfo,
            int deviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine($"Getting sensors for device ID: {deviceId}");

                // Get all sensors first
                var allSensors = await GetAllSensorsAsync(connectionInfo, cancellationToken);

                // Filter by device ID
                var filteredSensors = allSensors
                    .Where(s => s.DeviceId == deviceId)
                    .ToList();

                Debug.WriteLine($"Retrieved {filteredSensors.Count} sensors for device {deviceId}");
                return filteredSensors;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting sensors for device: {ex.Message}");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Helper method to retrieve sensors from the PRTG server.
        /// This stub should be replaced with an actual API call using GetPrtgClient(connectionInfo).
        /// </summary>
        private static async Task<List<PrtgSensor>> GetSensorsAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            Debug.WriteLine("Stub: GetSensorsAsync called");
            // TODO: Replace with an actual call to the PRTG client API, e.g.:
            // var client = GetPrtgClient(connectionInfo);
            // return await client.GetSensorsAsync(cancellationToken);
            return await Task.FromResult(new List<PrtgSensor>());
        }

        #endregion
    }
}
