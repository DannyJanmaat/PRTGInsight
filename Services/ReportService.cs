using OfficeOpenXml;
using PRTGInsight.Models;
using PRTGInsight.Services.Prtg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
// Remove iText imports and use a different approach for PDF

namespace PRTGInsight.Services
{
    public enum ReportFormat
    {
        Pdf,
        Excel,
        Csv
    }

    public enum ReportType
    {
        SensorStatus,
        DeviceStatus,
        UptimeReport,
        DowntimeReport,
        CustomReport
    }

    public class ReportService
    {
        private readonly PrtgService _prtgService;

        public ReportService(PrtgService prtgService)
        {
            _prtgService = prtgService;

            // Initialize EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public static async Task<string> GenerateReportAsync(
            ReportType reportType,
            ReportFormat format,
            ConnectionInfo connectionInfo,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                // Get data based on report type
                object data = await GetReportDataAsync(reportType, connectionInfo, startDate, endDate);

                // Generate the report in the requested format
                string filePath = await ExportReportAsync(data, reportType, format);

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating report: {ex.Message}", ex);
            }
        }

        private static async Task<object> GetReportDataAsync(
            ReportType reportType,
            ConnectionInfo connectionInfo,
            DateTime _,
            DateTime __)
        {
            CancellationToken cancellationToken = default;

            return reportType switch
            {
                ReportType.SensorStatus => connectionInfo.UseApiKey
                                            ? await PrtgService.GetSensorsAsync(
                                                new ConnectionInfo
                                                {
                                                    ServerUrl = connectionInfo.ServerUrl,
                                                    ApiKey = connectionInfo.ApiKey,
                                                    UseApiKey = true
                                                },
                                                cancellationToken)
                                            : (object)await PrtgService.GetSensorsWithCredentialsAsync(
                                                connectionInfo.ServerUrl,
                                                connectionInfo.Username,
                                                connectionInfo.Password,
                                                cancellationToken),
                ReportType.DeviceStatus => connectionInfo.UseApiKey
                                            ? await PrtgService.GetDevicesAsync(
                                                connectionInfo.ServerUrl,
                                                connectionInfo.ApiKey,
                                                cancellationToken)
                                            : (object)await PrtgService.GetDevicesWithCredentialsAsync(
                                                connectionInfo.ServerUrl,
                                                connectionInfo.Username,
                                                connectionInfo.Password,
                                                cancellationToken),
                // Other report types would have their own data retrieval logic
                _ => throw new NotImplementedException($"Report type {reportType} is not yet implemented."),
            };
        }

        private static async Task<string> ExportReportAsync(object data, ReportType reportType, ReportFormat format)
        {
            string fileName = $"PRTG_{reportType}_{DateTime.Now:yyyyMMdd_HHmmss}";
            string extension = GetFileExtension(format);
            string filePath = await GetSaveFilePathAsync(fileName, extension);

            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("No file path selected for saving the report.");
            }

            switch (format)
            {
                case ReportFormat.Excel:
                    await ExportToExcelAsync(data, reportType, filePath);
                    break;
                case ReportFormat.Csv:
                    await ExportToCsvAsync(data, reportType, filePath);
                    break;
                case ReportFormat.Pdf:
                    // For now, we'll use a simple approach for PDF
                    await ExportToSimplePdfAsync(data, reportType, filePath);
                    break;
                default:
                    throw new ArgumentException($"Unsupported report format: {format}");
            }

            return filePath;
        }

        private static async Task<string> GetSaveFilePathAsync(string fileName, string extension)
        {
            FileSavePicker savePicker = new()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = fileName
            };

            switch (extension.ToLower())
            {
                case ".pdf":
                    savePicker.FileTypeChoices.Add("PDF Document", [".pdf"]);
                    break;
                case ".xlsx":
                    savePicker.FileTypeChoices.Add("Excel Workbook", [".xlsx"]);
                    break;
                case ".csv":
                    savePicker.FileTypeChoices.Add("CSV File", [".csv"]);
                    break;
            }

            // Initialize the picker with the window handle
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker,
                WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));

            StorageFile file = await savePicker.PickSaveFileAsync();
            return file?.Path;
        }

        // Simplified PDF export that doesn't use iText
        private static async Task ExportToSimplePdfAsync(object data, ReportType reportType, string filePath)
        {
            // For now, we'll just create a simple text file with PDF extension
            // In a real app, you would use a PDF library that works well with your project

            using StreamWriter writer = new(filePath, false, Encoding.UTF8);

            await writer.WriteLineAsync($"PRTG {reportType} Report");
            await writer.WriteLineAsync($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await writer.WriteLineAsync();

            switch (reportType)
            {
                case ReportType.SensorStatus:
                    if (data is List<PrtgSensor> sensors)
                    {
                        await writer.WriteLineAsync("Sensors:");
                        await writer.WriteLineAsync("Name\tStatus\tDevice\tLast Value\tLast Check");

                        foreach (PrtgSensor sensor in sensors)
                        {
                            await writer.WriteLineAsync(
                                $"{sensor.Name}\t{sensor.Status}\t{sensor.Device}\t{sensor.LastValue}\t{sensor.LastCheckDateTime:yyyy-MM-dd HH:mm:ss}");
                        }

                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("Summary:");
                        await writer.WriteLineAsync($"Total Sensors: {sensors.Count}");
                        await writer.WriteLineAsync($"Up: {sensors.Count(s => s.Status.Equals("Up", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Warning: {sensors.Count(s => s.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Down: {sensors.Count(s => s.Status.Equals("Down", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Paused: {sensors.Count(s => s.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))}");
                    }
                    break;

                case ReportType.DeviceStatus:
                    if (data is List<PrtgDevice> devices)
                    {
                        await writer.WriteLineAsync("Devices:");
                        await writer.WriteLineAsync("Name\tStatus\tType\tMessage");

                        foreach (PrtgDevice device in devices)
                        {
                            await writer.WriteLineAsync(
                                $"{device.Name}\t{device.Status}\t{device.Type}\t{device.Message}");
                        }

                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("Summary:");
                        await writer.WriteLineAsync($"Total Devices: {devices.Count}");
                        await writer.WriteLineAsync($"Up: {devices.Count(d => d.Status.Equals("Up", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Warning: {devices.Count(d => d.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Down: {devices.Count(d => d.Status.Equals("Down", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Paused: {devices.Count(d => d.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))}");
                    }
                    break;

                default:
                    await writer.WriteLineAsync("Report type not implemented yet.");
                    break;
            }

            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Note: This is a simple text-based PDF. For a more professional PDF, please use Excel or CSV export.");
        }

        private static async Task ExportToExcelAsync(object data, ReportType reportType, string filePath)
        {
            using ExcelPackage package = new();

            // Create a worksheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add($"{reportType}");

            // Add title
            worksheet.Cells[1, 1].Value = $"PRTG {reportType} Report";
            worksheet.Cells[1, 1, 1, 5].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            worksheet.Cells[2, 1].Value = $"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells[2, 1, 2, 5].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Italic = true;

            // Create table based on report type
            switch (reportType)
            {
                case ReportType.SensorStatus:
                    if (data is List<PrtgSensor> sensors)
                    {
                        // Add headers
                        worksheet.Cells[4, 1].Value = "Name";
                        worksheet.Cells[4, 2].Value = "Status";
                        worksheet.Cells[4, 3].Value = "Device";
                        worksheet.Cells[4, 4].Value = "Last Value";
                        worksheet.Cells[4, 5].Value = "Last Check";

                        // Style headers
                        worksheet.Cells[4, 1, 4, 5].Style.Font.Bold = true;

                        // Add data rows
                        int row = 5;
                        foreach (PrtgSensor sensor in sensors)
                        {
                            worksheet.Cells[row, 1].Value = sensor.Name;
                            worksheet.Cells[row, 2].Value = sensor.Status;
                            worksheet.Cells[row, 3].Value = sensor.Device;
                            worksheet.Cells[row, 4].Value = sensor.LastValue;
                            worksheet.Cells[row, 5].Value = sensor.LastCheckDateTime;
                            worksheet.Cells[row, 5].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                            row++;
                        }

                        // Add summary
                        row += 2;
                        worksheet.Cells[row, 1].Value = "Summary:";
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                        row++;
                        worksheet.Cells[row, 1].Value = "Total Sensors:";
                        worksheet.Cells[row, 2].Value = sensors.Count;
                        row++;
                        worksheet.Cells[row, 1].Value = "Up:";
                        worksheet.Cells[row, 2].Value = sensors.Count(s => s.Status.Equals("Up", StringComparison.OrdinalIgnoreCase));
                        row++;
                        worksheet.Cells[row, 1].Value = "Warning:";
                        worksheet.Cells[row, 2].Value = sensors.Count(s => s.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase));
                        row++;
                        worksheet.Cells[row, 1].Value = "Down:";
                        worksheet.Cells[row, 2].Value = sensors.Count(s => s.Status.Equals("Down", StringComparison.OrdinalIgnoreCase));
                        row++;
                        worksheet.Cells[row, 1].Value = "Paused:";
                        worksheet.Cells[row, 2].Value = sensors.Count(s => s.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase));

                        // Auto-fit columns
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }
                    break;

                case ReportType.DeviceStatus:
                    if (data is List<PrtgDevice> devices)
                    {
                        // Add headers
                        worksheet.Cells[4, 1].Value = "Name";
                        worksheet.Cells[4, 2].Value = "Status";
                        worksheet.Cells[4, 3].Value = "Type";
                        worksheet.Cells[4, 4].Value = "Message";

                        // Style headers
                        worksheet.Cells[4, 1, 4, 4].Style.Font.Bold = true;

                        // Add data rows
                        int row = 5;
                        foreach (PrtgDevice device in devices)
                        {
                            worksheet.Cells[row, 1].Value = device.Name;
                            worksheet.Cells[row, 2].Value = device.Status;
                            worksheet.Cells[row, 3].Value = device.Type;
                            worksheet.Cells[row, 4].Value = device.Message;
                            row++;
                        }

                        // Add summary
                        row += 2;
                        worksheet.Cells[row, 1].Value = "Summary:";
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                        row++;
                        worksheet.Cells[row, 1].Value = "Total Devices:";
                        worksheet.Cells[row, 2].Value = devices.Count;
                        row++;
                        worksheet.Cells[row, 1].Value = "Up:";
                        worksheet.Cells[row, 2].Value = devices.Count(d => d.Status.Equals("Up", StringComparison.OrdinalIgnoreCase));
                        row++;
                        worksheet.Cells[row, 1].Value = "Warning:";
                        worksheet.Cells[row, 2].Value = devices.Count(d => d.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase));
                        row++;
                        worksheet.Cells[row, 1].Value = "Down:";
                        worksheet.Cells[row, 2].Value = devices.Count(d => d.Status.Equals("Down", StringComparison.OrdinalIgnoreCase));
                        row++;
                        worksheet.Cells[row, 1].Value = "Paused:";
                        worksheet.Cells[row, 2].Value = devices.Count(d => d.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase));

                        // Auto-fit columns
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }
                    break;

                // Other report types would have their own Excel generation logic
                default:
                    worksheet.Cells[4, 1].Value = "Report type not implemented yet.";
                    break;
            }

            // Save the Excel file
            await package.SaveAsAsync(new FileInfo(filePath));
        }

        private static async Task ExportToCsvAsync(object data, ReportType reportType, string filePath)
        {
            using StreamWriter writer = new(filePath, false, Encoding.UTF8);

            switch (reportType)
            {
                case ReportType.SensorStatus:
                    if (data is List<PrtgSensor> sensors)
                    {
                        // Write header
                        await writer.WriteLineAsync("Name,Status,Device,LastValue,LastCheck");

                        // Write data rows
                        foreach (PrtgSensor sensor in sensors)
                        {
                            await writer.WriteLineAsync(
                                $"\"{EscapeCsvField(sensor.Name)}\",\"{sensor.Status}\",\"{EscapeCsvField(sensor.Device)}\",\"{EscapeCsvField(sensor.LastValue)}\",\"{sensor.LastCheckDateTime:yyyy-MM-dd HH:mm:ss}\"");
                        }

                        // Write summary
                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("Summary:");
                        await writer.WriteLineAsync($"Total Sensors,{sensors.Count}");
                        await writer.WriteLineAsync($"Up,{sensors.Count(s => s.Status.Equals("Up", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Warning,{sensors.Count(s => s.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Down,{sensors.Count(s => s.Status.Equals("Down", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Paused,{sensors.Count(s => s.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))}");
                    }
                    break;

                case ReportType.DeviceStatus:
                    if (data is List<PrtgDevice> devices)
                    {
                        // Write header
                        await writer.WriteLineAsync("Name,Status,Type,Message");

                        // Write data rows
                        foreach (PrtgDevice device in devices)
                        {
                            await writer.WriteLineAsync(
                                $"\"{EscapeCsvField(device.Name)}\",\"{device.Status}\",\"{EscapeCsvField(device.Type)}\",\"{EscapeCsvField(device.Message)}\"");
                        }

                        // Write summary
                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("Summary:");
                        await writer.WriteLineAsync($"Total Devices,{devices.Count}");
                        await writer.WriteLineAsync($"Up,{devices.Count(d => d.Status.Equals("Up", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Warning,{devices.Count(d => d.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Down,{devices.Count(d => d.Status.Equals("Down", StringComparison.OrdinalIgnoreCase))}");
                        await writer.WriteLineAsync($"Paused,{devices.Count(d => d.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))}");
                    }
                    break;

                // Other report types would have their own CSV generation logic
                default:
                    await writer.WriteLineAsync("Report type not implemented yet.");
                    break;
            }
        }

        private static string GetFileExtension(ReportFormat format)
        {
            return format switch
            {
                ReportFormat.Pdf => ".pdf",
                ReportFormat.Excel => ".xlsx",
                ReportFormat.Csv => ".csv",
                _ => throw new ArgumentException($"Unsupported report format: {format}")
            };
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            // Replace double quotes with two double quotes
            return field.Replace("\"", "\"\"");
        }
    }
}
