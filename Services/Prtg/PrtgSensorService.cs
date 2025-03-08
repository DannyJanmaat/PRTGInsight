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
    public class PrtgSensorService : BasePrtgService
    {
        public static async Task<List<PrtgSensor>> GetSensorsAsync(ConnectionInfo connectionInfo, CancellationToken _ = default)
        {
            try
            {
                Debug.WriteLine("Getting sensors");

                var client = GetPrtgClient(connectionInfo);

                // Get all sensors
                var sensors = await client.GetSensorsAsync(_);

                // Convert to our model
                var result = sensors.Select(s => new PrtgSensor
                {
                    Id = s.Id,
                    Name = s.Name,
                    Status = s.Status.ToString(),
                    Device = s.Device,
                    DeviceId = s.ParentId,
                    Type = s.Type.ToString(),
                    LastValue = s.LastValue.HasValue ? s.LastValue.Value.ToString() : "N/A",
                    Message = s.Message,
                    LastCheck = s.LastCheck.ToString(),
                    IsActive = s.Active,
                    Priority = (int)s.Priority
                }).ToList();

                Debug.WriteLine($"Retrieved {result.Count} sensors");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting sensors: {ex.Message}");
                throw;
            }
        }

        public static async Task<PrtgSensorDetails> GetSensorDetailsAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken _ = default)
        {
            try
            {
                Debug.WriteLine($"Getting details for sensor {sensorId}");

                var client = GetPrtgClient(connectionInfo);

                // Get sensor details
                var sensor = await client.GetSensorAsync(sensorId, _) ?? throw new Exception($"Sensor {sensorId} not found");

                // Convert to our model
                var result = new PrtgSensorDetails
                {
                    Id = sensor.Id,
                    Name = sensor.Name,
                    Status = sensor.Status.ToString(),
                    Device = sensor.Device,
                    DeviceId = sensor.ParentId,
                    Type = sensor.Type.ToString(),
                    LastValue = sensor.LastValue.HasValue ? sensor.LastValue.Value.ToString() : "N/A",
                    Message = sensor.Message,
                    LastCheck = sensor.LastCheck.ToString(),
                    Uptime = sensor.Uptime?.ToString() ?? "N/A",
                    Downtime = sensor.Downtime?.ToString() ?? "N/A",
                    Interval = sensor.Interval.ToString(),
                    AccessRights = sensor.Access.ToString(),
                    Tags = string.Join(", ", sensor.Tags)
                };

                Debug.WriteLine($"Retrieved details for sensor {sensorId}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting sensor details: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<PrtgSensorHistoryItem>> GetSensorHistoryAsync(
            ConnectionInfo connectionInfo,
            int sensorId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken _ = default)
        {
            try
            {
                Debug.WriteLine($"Getting history for sensor {sensorId} from {startDate} to {endDate}");

                var client = GetPrtgClient(connectionInfo);

                // Get sensor history - PrtgAPI doesn't directly support date range for history
                // We'll get all history and filter it ourselves
                var history = await client.GetSensorHistoryAsync(sensorId, token: _);

                // Filter by date range
                var filteredHistory = history.Where(h =>
                    h.DateTime >= startDate && h.DateTime <= endDate).ToList();

                // Replace the line causing the error in the GetSensorHistoryAsync method
                var result = filteredHistory.Select(h => new PrtgSensorHistoryItem
                {
                    DateTime = h.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    DateTimeValue = h.DateTime,
                    Value = h.ChannelRecords.FirstOrDefault()?.Value.ToString() ?? "N/A", // Assuming Value is in ChannelRecords
                    Status = "N/A", // Placeholder as Status is not available in SensorHistoryRecord
                    Message = "N/A" // Placeholder as Message is not available in ChannelHistoryRecord
                }).ToList();

                Debug.WriteLine($"Retrieved {result.Count} history items for sensor {sensorId}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting sensor history: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> GetSensorGraphUrlAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken _ = default)
        {
            try
            {
                Debug.WriteLine($"Getting graph URL for sensor {sensorId}");

                // Build the graph URL
                string serverUrl = connectionInfo.ServerUrl.TrimEnd('/');
                string graphUrl;

                if (connectionInfo.UseApiKey)
                {
                    graphUrl = $"{serverUrl}/chart.png?id={sensorId}&width=800&height=300&apitoken={Uri.EscapeDataString(connectionInfo.ApiKey)}";
                }
                else
                {
                    graphUrl = $"{serverUrl}/chart.png?id={sensorId}&width=800&height=300&username={Uri.EscapeDataString(connectionInfo.Username)}&password={Uri.EscapeDataString(connectionInfo.Password)}";
                }

                Debug.WriteLine($"Graph URL: {graphUrl}");
                return await Task.FromResult(graphUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting sensor graph URL: {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> ScanSensorNowAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken _ = default)
        {
            try
            {
                Debug.WriteLine($"Scanning sensor {sensorId} now");

                var client = GetPrtgClient(connectionInfo);

                // Scan the sensor
                await client.RefreshObjectAsync([sensorId], token: _);

                Debug.WriteLine($"Scan initiated for sensor {sensorId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scanning sensor: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> PauseSensorAsync(ConnectionInfo connectionInfo, int sensorId, string pauseMessage = "Paused by PRTG Insight", CancellationToken _ = default)
        {
            try
            {
                Debug.WriteLine($"Pausing sensor {sensorId}");

                var client = GetPrtgClient(connectionInfo);

                // Pause the sensor
                await client.PauseObjectAsync(sensorId, null, pauseMessage, _);

                Debug.WriteLine($"Sensor {sensorId} paused");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error pausing sensor: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> ResumeSensorAsync(ConnectionInfo connectionInfo, int sensorId, CancellationToken _ = default)
        {
            try
            {
                Debug.WriteLine($"Resuming sensor {sensorId}");

                var client = GetPrtgClient(connectionInfo);

                // Resume the sensor
                await client.ResumeObjectAsync([sensorId], token: _);

                Debug.WriteLine($"Sensor {sensorId} resumed");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resuming sensor: {ex.Message}");
                return false;
            }
        }
    }
}
