using PrtgAPI;
using PRTGInsight.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PRTGInsight.Services.Prtg
{
    public partial class PrtgDeviceService : BasePrtgService
    {
        #region Additional Device Methods

        /// <summary>
        /// Gets all devices from the PRTG server.
        /// </summary>
        /// <param name="connectionInfo">Connection information for the PRTG server</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of all devices</returns>
        public static async Task<List<PrtgDevice>> GetAllDevicesAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine("Getting all devices");

                // Use the helper method to get devices
                var devices = await GetDevicesAsync(connectionInfo, cancellationToken);

                Debug.WriteLine($"Retrieved {devices.Count} devices");
                return devices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting all devices: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets devices by status (up, down, warning, etc.)
        /// </summary>
        /// <param name="connectionInfo">Connection information for the PRTG server</param>
        /// <param name="status">Status to filter by (Up, Down, Warning, etc.)</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of filtered devices</returns>
        public static async Task<List<PrtgDevice>> GetDevicesByStatusAsync(
            ConnectionInfo connectionInfo,
            string status,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine($"Getting devices with status: {status}");

                // Get all devices first
                var allDevices = await GetAllDevicesAsync(connectionInfo, cancellationToken);

                // Filter by status if specified
                if (!string.IsNullOrWhiteSpace(status))
                {
                    allDevices = [.. allDevices.Where(d => d.Status.Equals(status, StringComparison.OrdinalIgnoreCase))];
                }

                Debug.WriteLine($"Retrieved {allDevices.Count} devices with status {status}");
                return allDevices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting devices by status: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets devices by type.
        /// </summary>
        /// <param name="connectionInfo">Connection information for the PRTG server</param>
        /// <param name="deviceType">The device type to filter by</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of filtered devices</returns>
        public static async Task<List<PrtgDevice>> GetDevicesByTypeAsync(
            ConnectionInfo connectionInfo,
            string deviceType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine($"Getting devices of type: {deviceType}");

                // Get all devices first
                var allDevices = await GetAllDevicesAsync(connectionInfo, cancellationToken);

                // Filter devices by device type
                var filteredDevices = allDevices
                    .Where(d => d.Type.Equals(deviceType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Debug.WriteLine($"Retrieved {filteredDevices.Count} devices of type {deviceType}");
                return filteredDevices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting devices by type: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets devices by tag.
        /// </summary>
        /// <param name="connectionInfo">Connection information for the PRTG server</param>
        /// <param name="tag">The tag to filter by</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of filtered devices</returns>
        public static async Task<List<PrtgDevice>> GetDevicesByTagAsync(
            ConnectionInfo connectionInfo,
            string tag,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine($"Getting devices with tag: {tag}");

                // Get all devices first
                var allDevices = await GetAllDevicesAsync(connectionInfo, cancellationToken);

                // Filter by tag if available
                var filteredDevices = allDevices
                    .Where(d => d.Tags != null && d.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Debug.WriteLine($"Retrieved {filteredDevices.Count} devices with tag {tag}");
                return filteredDevices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting devices by tag: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Helper method to retrieve devices from the PRTG server using the provided connection information.
        /// This is a stub that should be replaced with the actual API call.
        /// </summary>
        private static async Task<List<PrtgDevice>> GetDevicesAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            // TODO: Replace the following stub with a call to the actual PRTG client.
            // For example:
            // var client = GetPrtgClient(connectionInfo);
            // return await client.GetDevicesAsync(cancellationToken);
            Debug.WriteLine("Stub: GetDevicesAsync called");
            return await Task.FromResult(new List<PrtgDevice>());
        }

        #endregion
    }
}
