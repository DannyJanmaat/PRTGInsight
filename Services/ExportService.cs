using PRTGInsight.Models;
using PRTGInsight.Services.Prtg;
using PRTGInsight.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace PRTGInsight.Services
{
    public static class ExportService
    {
        private static readonly string ExportFolder = "Exports";
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        public static async Task<bool> ExportDashboardDataAsync(ConnectionInfo connectionInfo)
        {
            try
            {
                // Get dashboard data
                PrtgSensorStatus sensorStatus = await PrtgDashboardService.GetSensorStatusAsync(connectionInfo);
                PrtgSystemStatus systemStatus = await PrtgDashboardService.GetSystemStatusAsync(connectionInfo);
                List<PrtgAlert> alerts = await PrtgDashboardService.GetRecentAlertsAsync(connectionInfo);

                // Create export data object
                var exportData = new
                {
                    ExportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Server = connectionInfo.ServerUrl,
                    connectionInfo.PrtgVersion,
                    SensorStatus = sensorStatus,
                    SystemStatus = systemStatus,
                    Alerts = alerts
                };

                // Create export file
                string fileName = $"Dashboard_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                await SaveExportFileAsync(fileName, exportData);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting dashboard data: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> ExportSensorsAsync(ConnectionInfo connectionInfo)
        {
            try
            {
                // Get all sensors
                List<PrtgSensor> sensors = await PrtgSensorService.GetAllSensorsAsync(connectionInfo);

                // Create export data object
                var exportData = new
                {
                    ExportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Server = connectionInfo.ServerUrl,
                    connectionInfo.PrtgVersion,
                    Sensors = sensors
                };

                // Create export file
                string fileName = $"Sensors_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                await SaveExportFileAsync(fileName, exportData);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting sensors: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> ExportDevicesAsync(ConnectionInfo connectionInfo)
        {
            try
            {
                // Get all devices
                List<PrtgDevice> devices = await PrtgDeviceService.GetAllDevicesAsync(connectionInfo);

                // Create export data object
                var exportData = new
                {
                    ExportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Server = connectionInfo.ServerUrl,
                    connectionInfo.PrtgVersion,
                    Devices = devices
                };

                // Create export file
                string fileName = $"Devices_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                await SaveExportFileAsync(fileName, exportData);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting devices: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> ExportAlertsAsync(ConnectionInfo connectionInfo)
        {
            try
            {
                // Get all alerts using PrtgDashboardService instead of PrtgAlertService
                List<PrtgAlert> alerts = await PrtgDashboardService.GetRecentAlertsAsync(connectionInfo);

                // Create export data object
                var exportData = new
                {
                    ExportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Server = connectionInfo.ServerUrl,
                    connectionInfo.PrtgVersion,
                    Alerts = alerts
                };

                // Create export file
                string fileName = $"Alerts_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                await SaveExportFileAsync(fileName, exportData);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting alerts: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> CreateCustomExportAsync(
            ConnectionInfo connectionInfo,
            Dictionary<string, bool> options)
        {
            try
            {
                Dictionary<string, object> exportData = new()
                {
                    { "ExportDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "Server", connectionInfo.ServerUrl },
                    { "PrtgVersion", connectionInfo.PrtgVersion }
                };

                // Add selected data to export
                if (options["Sensors"])
                {
                    List<PrtgSensor> sensors = await PrtgSensorService.GetAllSensorsAsync(connectionInfo);
                    exportData.Add("Sensors", sensors);
                }

                if (options["Devices"])
                {
                    List<PrtgDevice> devices = await PrtgDeviceService.GetAllDevicesAsync(connectionInfo);
                    exportData.Add("Devices", devices);
                }

                if (options["Alerts"])
                {
                    // Use dashboard service to get alerts as a replacement for PrtgAlertService
                    List<PrtgAlert> alerts = await PrtgDashboardService.GetRecentAlertsAsync(connectionInfo);
                    exportData.Add("Alerts", alerts);
                }

                if (options["SystemStatus"])
                {
                    PrtgSystemStatus systemStatus = await PrtgDashboardService.GetSystemStatusAsync(connectionInfo);
                    exportData.Add("SystemStatus", systemStatus);
                }

                // Determine file format and name
                string extension = options["UseXml"] ? "xml" :
                                  (options["Format"] ? "csv" : "json");

                string fileName = $"Custom_Export_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";

                // Save based on format
                if (options["UseXml"])
                {
                    // Save as XML
                    await SaveExportXmlAsync(fileName, exportData);
                }
                else if (options["Format"])
                {
                    // Save as CSV
                    await SaveExportCsvAsync(fileName, exportData);
                }
                else
                {
                    // Save as JSON
                    await SaveExportFileAsync(fileName, exportData);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating custom export: {ex.Message}");
                return false;
            }
        }

        private static async Task SaveExportFileAsync(string fileName, object data)
        {
            // Ensure export directory exists
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder exportFolder = await localFolder.CreateFolderAsync(
                ExportFolder, CreationCollisionOption.OpenIfExists);

            // Create file
            StorageFile file = await exportFolder.CreateFileAsync(
                fileName, CreationCollisionOption.ReplaceExisting);

            // Serialize data to JSON
            string json = JsonSerializer.Serialize(data, _jsonSerializerOptions);

            // Write to file
            await FileIO.WriteTextAsync(file, json);
        }

        private static async Task SaveExportCsvAsync(string fileName, Dictionary<string, object> data)
        {
            // Implement CSV export logic
            // This is a simplified example - a real implementation would need to handle
            // complex data structures and proper CSV formatting

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder exportFolder = await localFolder.CreateFolderAsync(
                ExportFolder, CreationCollisionOption.OpenIfExists);

            StorageFile file = await exportFolder.CreateFileAsync(
                fileName, CreationCollisionOption.ReplaceExisting);

            using Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            using StreamWriter writer = new(stream.AsStream());
            // Write header info
            await writer.WriteLineAsync($"Export Date,{data["ExportDate"]}");
            await writer.WriteLineAsync($"Server,{data["Server"]}");
            await writer.WriteLineAsync($"PRTG Version,{data["PrtgVersion"]}");
            await writer.WriteLineAsync();

            // Write each data section
            // Note: This is simplified and would need to be expanded for real data structures
            foreach (KeyValuePair<string, object> kvp in data)
            {
                if (kvp.Key is not "ExportDate" and not "Server" and not "PrtgVersion")
                {
                    await writer.WriteLineAsync($"--- {kvp.Key} ---");
                    // Custom logic to format each data type as CSV would go here
                    await writer.WriteLineAsync();
                }
            }
        }

        private static async Task SaveExportXmlAsync(string fileName, Dictionary<string, object> data)
        {
            // Implement XML export logic
            // This is a simplified example - a real implementation would use proper XML formatting

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder exportFolder = await localFolder.CreateFolderAsync(
                ExportFolder, CreationCollisionOption.OpenIfExists);

            StorageFile file = await exportFolder.CreateFileAsync(
                fileName, CreationCollisionOption.ReplaceExisting);

            using Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            using StreamWriter writer = new(stream.AsStream());
            await writer.WriteLineAsync("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            await writer.WriteLineAsync("<PRTGExport>");

            await writer.WriteLineAsync($"  <ExportDate>{data["ExportDate"]}</ExportDate>");
            await writer.WriteLineAsync($"  <Server>{data["Server"]}</Server>");
            await writer.WriteLineAsync($"  <PrtgVersion>{data["PrtgVersion"]}</PrtgVersion>");

            // Write each data section
            // Note: This is simplified and would need to be expanded for real data structures
            foreach (KeyValuePair<string, object> kvp in data)
            {
                if (kvp.Key is not "ExportDate" and not "Server" and not "PrtgVersion")
                {
                    await writer.WriteLineAsync($"  <{kvp.Key}>");
                    // Custom logic to format each data type as XML would go here
                    await writer.WriteLineAsync($"  </{kvp.Key}>");
                }
            }

            await writer.WriteLineAsync("</PRTGExport>");
        }

        public static async Task<List<ExportInfo>> GetRecentExportsAsync()
        {
            List<ExportInfo> exports = [];

            try
            {
                // Get export folder
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder exportFolder = await localFolder.CreateFolderAsync(
                    ExportFolder, CreationCollisionOption.OpenIfExists);

                // Get all files in the export folder
                IReadOnlyList<StorageFile> files = await exportFolder.GetFilesAsync();

                // Convert to ExportInfo objects
                foreach (StorageFile file in files)
                {
                    Windows.Storage.FileProperties.BasicProperties properties = await file.GetBasicPropertiesAsync();

                    string exportType = "Unknown";
                    if (file.Name.StartsWith("Dashboard_"))
                    {
                        exportType = "Dashboard";
                    }
                    else if (file.Name.StartsWith("Sensors_"))
                    {
                        exportType = "Sensors";
                    }
                    else if (file.Name.StartsWith("Devices_"))
                    {
                        exportType = "Devices";
                    }
                    else if (file.Name.StartsWith("Alerts_"))
                    {
                        exportType = "Alerts";
                    }
                    else if (file.Name.StartsWith("Custom_"))
                    {
                        exportType = "Custom";
                    }

                    exports.Add(new ExportInfo
                    {
                        FileName = file.Name,
                        FilePath = file.Path,
                        ExportType = exportType,
                        ExportDate = properties.DateModified.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        FileSizeBytes = (long)properties.Size,
                        FileSizeFormatted = FormatFileSize(properties.Size)
                    });
                }

                // Sort by date, newest first
                exports.Sort((a, b) => DateTime.Parse(b.ExportDate).CompareTo(DateTime.Parse(a.ExportDate)));

                return exports;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting recent exports: {ex.Message}");
                return exports; // Return empty list on error
            }
        }

        public static async Task<bool> OpenExportFileAsync(string filePath)
        {
            try
            {
                // Get the file
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);

                // Launch the file with the default handler
                bool success = await Launcher.LaunchFileAsync(file);

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening export file: {ex.Message}");
                return false;
            }
        }

        private static string FormatFileSize(ulong sizeInBytes)
        {
            string[] sizes = ["B", "KB", "MB", "GB"];
            double formattedSize = sizeInBytes;
            int sizeIndex = 0;

            while (formattedSize >= 1024 && sizeIndex < sizes.Length - 1)
            {
                formattedSize /= 1024;
                sizeIndex++;
            }

            return $"{formattedSize:0.##} {sizes[sizeIndex]}";
        }
    }
}
