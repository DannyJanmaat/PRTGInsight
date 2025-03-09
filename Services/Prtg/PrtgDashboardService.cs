using PrtgAPI;
using PRTGInsight.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PRTGInsight.Services.Prtg
{
    public class PrtgDashboardService : BasePrtgService
    {
        public static async Task<PrtgSensorStatus> GetSensorStatusAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine("[DEBUG] Getting sensor status from PRTG");

                PrtgClient client = GetPrtgClient(connectionInfo);
                List<Sensor> sensors = await client.GetSensorsAsync(cancellationToken);

                // Calculate sensor counts
                int totalSensors = sensors.Count;
                int upSensors = sensors.Count(s => s.Status == Status.Up);
                int downSensors = sensors.Count(s => s.Status is Status.Down or Status.DownAcknowledged);
                int warningSensors = sensors.Count(s => s.Status == Status.Warning);
                int pausedSensors = sensors.Count(s => s.Status is Status.PausedByUser or
                                                       Status.PausedByDependency or
                                                       Status.PausedBySchedule);

                List<Device> devices = await client.GetDevicesAsync(cancellationToken);
                int totalDevices = devices.Count;

                Debug.WriteLine($"[DEBUG] Sensor status: Total={totalSensors}, Up={upSensors}, Down={downSensors}, Warning={warningSensors}, Paused={pausedSensors}");

                return new PrtgSensorStatus
                {
                    TotalSensors = totalSensors,
                    UpSensors = upSensors,
                    DownSensors = downSensors,
                    WarningSensors = warningSensors,
                    PausedSensors = pausedSensors,
                    TotalDevices = totalDevices
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error getting sensor status: {ex.Message}");
                return new PrtgSensorStatus();
            }
        }

        public static async Task<PrtgSystemStatus> GetSystemStatusAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine("[DEBUG] Getting system status from PRTG");

                PrtgClient client = GetPrtgClient(connectionInfo);
                ServerStatus status = await client.GetStatusAsync(cancellationToken);

                // Use reflection to obtain CPU, Memory, and Uptime if available.
                string cpuLoad = status.GetType().GetProperty("CpuUsage") != null
                    ? $"{status.GetType().GetProperty("CpuUsage")?.GetValue(status)}%"
                    : "N/A";
                string memoryUsage = status.GetType().GetProperty("MemoryUsage") != null
                    ? $"{status.GetType().GetProperty("MemoryUsage")?.GetValue(status)}%"
                    : "N/A";

                TimeSpan uptimeSpan = TimeSpan.Zero;
                if (status.GetType().GetProperty("Uptime") != null)
                {
                    object uptimeObj = status.GetType().GetProperty("Uptime")?.GetValue(status);
                    if (uptimeObj is TimeSpan ts)
                    {
                        uptimeSpan = ts;
                    }
                }
                string uptime = uptimeSpan != TimeSpan.Zero ? FormatUptime(uptimeSpan) : "N/A";

                Debug.WriteLine($"[DEBUG] System status: CPU={cpuLoad}, Memory={memoryUsage}, Uptime={uptime}");

                return new PrtgSystemStatus
                {
                    CpuLoadValue = cpuLoad,
                    MemoryUsageValue = memoryUsage,
                    UptimeValue = uptime,
                    Version = status.Version.ToString()
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error getting system status: {ex.Message}");
                return new PrtgSystemStatus
                {
                    CpuLoadValue = "N/A",
                    MemoryUsageValue = "N/A",
                    UptimeValue = "N/A",
                    Version = connectionInfo.PrtgVersion ?? "Unknown"
                };
            }
        }

        public static async Task<List<PrtgAlert>> GetRecentAlertsAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine("[DEBUG] Getting recent alerts from PRTG");

                PrtgClient client = GetPrtgClient(connectionInfo);
                // Call GetLogsAsync with the cancellation token
                List<Log> logs = await client.GetLogsAsync(recordAge: RecordAge.LastWeek, count: 20, status: null, token: cancellationToken);

                List<PrtgAlert> alerts = [.. logs
                    .Where(log =>
                    {
                        string type = log.Type.ToString().ToLower();
                        return type is "notification" or "error" or "warning";
                    })
                    .Select(log => new PrtgAlert
                    {
                        Message = log.Message,
                        TimeStamp = log.DateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        Status = GetAlertStatusFromLog(log),
                        Priority = GetAlertPriorityFromLog(log)
                    })];

                Debug.WriteLine($"[DEBUG] Found {alerts.Count} alerts");
                return alerts;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error getting alerts: {ex.Message}");
                return [];
            }
        }

        private static string FormatUptime(TimeSpan uptime)
        {
            return uptime.Days > 0
                ? $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m"
                : uptime.Hours > 0 ? $"{uptime.Hours}h {uptime.Minutes}m" : $"{uptime.Minutes}m {uptime.Seconds}s";
        }

        private static string GetAlertStatusFromLog(dynamic log)
        {
            string type = log.Type.ToString().ToLower();
            return type switch
            {
                "error" => "down",
                "warning" => "warning",
                "notification" => "up",
                _ => "info"
            };
        }

        private static string GetAlertPriorityFromLog(dynamic log)
        {
            string type = log.Type.ToString().ToLower();
            return type switch
            {
                "error" => "high",
                "warning" => "medium",
                _ => "low"
            };
        }
    }
}