using OfficeOpenXml;
using PRTGInsight.Models;
using PRTGInsight.Services.Prtg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PRTGInsight.Services.Reports
{
    public enum ReportType
    {
        SensorStatus,
        DeviceStatus,
        ProbeReport,
        GroupReport,
        CustomReport,
        UptimeReport,
        PerformanceReport
    }

    public enum ReportFormat
    {
        PDF,
        Excel,
        CSV,
        HTML
    }

    public class ReportGenerator
    {
        private readonly PrtgService _prtgService;
        private readonly ConnectionInfo _connectionInfo;

        public ReportGenerator(ConnectionInfo connectionInfo)
        {
            _prtgService = new PrtgService();
            _connectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));

            // Initialize EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<string> GenerateReportAsync(
            ReportType reportType,
            ReportFormat format,
            DateTime startDate,
            DateTime endDate,
            int? objectId = null)
        {
            try
            {
                // Collect data based on report type
                object reportData = await GetReportDataAsync(reportType, startDate, endDate, objectId);

                // Generate file path for saving
                string fileName = $"PRTG_{reportType}_{DateTime.Now:yyyyMMdd_HHmmss}";
                string extension = GetFileExtension(format);
                string filePath = await GetSaveFilePathAsync(fileName, extension);

                if (string.IsNullOrEmpty(filePath))
                {
                    return null; // User cancelled
                }

                // Generate report in the selected format
                switch (format)
                {
                    case ReportFormat.Excel:
                        await GenerateExcelReportAsync(reportData, reportType, filePath);
                        break;
                    case ReportFormat.CSV:
                        await GenerateCSVReportAsync(reportData, reportType, filePath);
                        break;
                    case ReportFormat.PDF:
                        await GeneratePDFReportAsync(reportData, reportType, filePath);
                        break;
                    case ReportFormat.HTML:
                        await GenerateHTMLReportAsync(reportData, reportType, filePath);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported report format: {format}");
                }

                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating report: {ex.Message}");
                throw;
            }
        }

        private async Task<object> GetReportDataAsync(
            ReportType reportType,
            DateTime startDate,
            DateTime endDate,
            int? objectId)
        {
            CancellationToken cancellationToken = default;

            switch (reportType)
            {
                case ReportType.SensorStatus:
                    return await PrtgService.GetSensorsAsync(_connectionInfo, cancellationToken);

                case ReportType.DeviceStatus:
                    return await PrtgService.GetDevicesAsync(_connectionInfo, cancellationToken);

                case ReportType.ProbeReport:
                    if (!objectId.HasValue)
                        throw new ArgumentException("ProbeReport requires an objectId");
                    // Placeholder for probe-specific report data
                    return await PrtgService.GetSensorsAsync(_connectionInfo, cancellationToken);

                case ReportType.GroupReport:
                    if (!objectId.HasValue)
                        throw new ArgumentException("GroupReport requires an objectId");
                    // Placeholder for group-specific report data
                    return await PrtgService.GetSensorsAsync(_connectionInfo, cancellationToken);

                case ReportType.UptimeReport:
                    // Placeholder for uptime report data
                    return await PrtgService.GetSensorsAsync(_connectionInfo, cancellationToken);

                case ReportType.PerformanceReport:
                    // Placeholder for performance report data
                    return await PrtgService.GetSensorsAsync(_connectionInfo, cancellationToken);

                case ReportType.CustomReport:
                    // Placeholder for custom report data
                    return await PrtgService.GetSensorsAsync(_connectionInfo, cancellationToken);

                default:
                    throw new ArgumentException($"Unsupported report type: {reportType}");
            }
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
                case ".html":
                    savePicker.FileTypeChoices.Add("HTML Document", [".html"]);
                    break;
            }

            // Initialize the picker with the window handle
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker,
                WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));

            StorageFile file = await savePicker.PickSaveFileAsync();
            return file?.Path;
        }

        private static string GetFileExtension(ReportFormat format)
        {
            return format switch
            {
                ReportFormat.PDF => ".pdf",
                ReportFormat.Excel => ".xlsx",
                ReportFormat.CSV => ".csv",
                ReportFormat.HTML => ".html",
                _ => throw new ArgumentException($"Unsupported report format: {format}")
            };
        }

        private async Task GenerateExcelReportAsync(object data, ReportType reportType, string filePath)
        {
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(reportType.ToString());

            // Add title and header
            worksheet.Cells[1, 1].Value = $"PRTG {reportType} Report";
            worksheet.Cells[1, 1, 1, 5].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            worksheet.Cells[2, 1].Value = $"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells[2, 1, 2, 5].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Italic = true;

            // Add data based on report type
            switch (reportType)
            {
                case ReportType.SensorStatus:
                    if (data is List<PrtgSensor> sensors)
                    {
                        await GenerateSensorStatusExcelReportAsync(worksheet, sensors);
                    }
                    break;
                case ReportType.DeviceStatus:
                    if (data is List<PrtgDevice> devices)
                    {
                        await GenerateDeviceStatusExcelReportAsync(worksheet, devices);
                    }
                    break;
                    // Add more cases as needed
            }

            // Save the Excel file
            await package.SaveAsAsync(new FileInfo(filePath));
        }

        private static async Task GenerateSensorStatusExcelReportAsync(ExcelWorksheet worksheet, List<PrtgSensor> sensors)
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
                worksheet.Cells[row, 5].Value = sensor.LastCheck;
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

            await Task.CompletedTask; // Just to keep it async
        }

        private static async Task GenerateDeviceStatusExcelReportAsync(ExcelWorksheet worksheet, List<PrtgDevice> devices)
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

            await Task.CompletedTask; // Just to keep it async
        }

        private async Task GenerateCSVReportAsync(object data, ReportType reportType, string filePath)
        {
            using StreamWriter writer = new(filePath, false, System.Text.Encoding.UTF8);

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
                                $"\"{EscapeCsvField(sensor.Name)}\",\"{sensor.Status}\",\"{EscapeCsvField(sensor.Device)}\",\"{EscapeCsvField(sensor.LastValue)}\",\"{sensor.LastCheck}\"");
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
                // Add more cases as needed
                default:
                    await writer.WriteLineAsync("Report type not implemented yet");
                    break;
            }
        }

        private static async Task GeneratePDFReportAsync(object data, ReportType reportType, string filePath)
        {
            // For now, create a placeholder for PDF reporting
            // In a full implementation, you would use a PDF library like iText7 or similar

            using StreamWriter writer = new(filePath, false, System.Text.Encoding.UTF8);

            await writer.WriteLineAsync($"PRTG {reportType} Report");
            await writer.WriteLineAsync($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("PDF report generation will be implemented in a future update.");
            await writer.WriteLineAsync("Please use Excel or CSV format for now.");

            // In future, implement PDF generation using a library such as iText7
        }

        private static async Task GenerateHTMLReportAsync(object data, ReportType reportType, string filePath)
        {
            using StreamWriter writer = new(filePath, false, System.Text.Encoding.UTF8);

            // HTML header
            await writer.WriteLineAsync("<!DOCTYPE html>");
            await writer.WriteLineAsync("<html lang=\"en\">");
            await writer.WriteLineAsync("<head>");
            await writer.WriteLineAsync("    <meta charset=\"UTF-8\">");
            await writer.WriteLineAsync("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            await writer.WriteLineAsync($"    <title>PRTG {reportType} Report</title>");
            await writer.WriteLineAsync("    <style>");
            await writer.WriteLineAsync("        body { font-family: Arial, sans-serif; margin: 20px; }");
            await writer.WriteLineAsync("        h1 { color: #0066cc; }");
            await writer.WriteLineAsync("        table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
            await writer.WriteLineAsync("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            await writer.WriteLineAsync("        th { background-color: #f2f2f2; }");
            await writer.WriteLineAsync("        tr:nth-child(even) { background-color: #f9f9f9; }");
            await writer.WriteLineAsync("        .up { color: green; }");
            await writer.WriteLineAsync("        .warning { color: orange; }");
            await writer.WriteLineAsync("        .down { color: red; }");
            await writer.WriteLineAsync("        .paused { color: gray; }");
            await writer.WriteLineAsync("        .summary { margin-top: 30px; font-weight: bold; }");
            await writer.WriteLineAsync("    </style>");
            await writer.WriteLineAsync("</head>");
            await writer.WriteLineAsync("<body>");
            await writer.WriteLineAsync($"    <h1>PRTG {reportType} Report</h1>");
            await writer.WriteLineAsync($"    <p>Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

            // Report content based on type
            switch (reportType)
            {
                case ReportType.SensorStatus:
                    if (data is List<PrtgSensor> sensors)
                    {
                        await writer.WriteLineAsync("    <table>");
                        await writer.WriteLineAsync("        <thead>");
                        await writer.WriteLineAsync("            <tr>");
                        await writer.WriteLineAsync("                <th>Name</th>");
                        await writer.WriteLineAsync("                <th>Status</th>");
                        await writer.WriteLineAsync("                <th>Device</th>");
                        await writer.WriteLineAsync("                <th>Last Value</th>");
                        await writer.WriteLineAsync("                <th>Last Check</th>");
                        await writer.WriteLineAsync("            </tr>");
                        await writer.WriteLineAsync("        </thead>");
                        await writer.WriteLineAsync("        <tbody>");

                        foreach (PrtgSensor sensor in sensors)
                        {
                            string statusClass = sensor.Status.ToLower() switch
                            {
                                "up" => "up",
                                "warning" => "warning",
                                "down" => "down",
                                "paused" => "paused",
                                _ => ""
                            };

                            await writer.WriteLineAsync("            <tr>");
                            await writer.WriteLineAsync($"                <td>{EscapeHtml(sensor.Name)}</td>");
                            await writer.WriteLineAsync($"                <td class=\"{statusClass}\">{EscapeHtml(sensor.Status)}</td>");
                            await writer.WriteLineAsync($"                <td>{EscapeHtml(sensor.Device)}</td>");
                            await writer.WriteLineAsync($"                <td>{EscapeHtml(sensor.LastValue)}</td>");
                            await writer.WriteLineAsync($"                <td>{EscapeHtml(sensor.LastCheck)}</td>");
                            await writer.WriteLineAsync("            </tr>");
                        }

                        await writer.WriteLineAsync("        </tbody>");
                        await writer.WriteLineAsync("    </table>");

                        // Summary
                        await writer.WriteLineAsync("    <div class=\"summary\">");
                        await writer.WriteLineAsync("        <h2>Summary</h2>");
                        await writer.WriteLineAsync("        <table>");
                        await writer.WriteLineAsync($"            <tr><td>Total Sensors:</td><td>{sensors.Count}</td></tr>");
                        await writer.WriteLineAsync($"            <tr><td>Up:</td><td>{sensors.Count(s => s.Status.Equals("Up", StringComparison.OrdinalIgnoreCase))}</td></tr>");
                        await writer.WriteLineAsync($"            <tr><td>Warning:</td><td>{sensors.Count(s => s.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase))}</td></tr>");
                        await writer.WriteLineAsync($"            <tr><td>Down:</td><td>{sensors.Count(s => s.Status.Equals("Down", StringComparison.OrdinalIgnoreCase))}</td></tr>");
                        await writer.WriteLineAsync($"            <tr><td>Paused:</td><td>{sensors.Count(s => s.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))}</td></tr>");
                        await writer.WriteLineAsync("        </table>");
                        await writer.WriteLineAsync("    </div>");
                    }
                    break;

                // Add more cases as needed
                default:
                    await writer.WriteLineAsync("    <p>Report type not fully implemented yet.</p>");
                    break;
            }

            // HTML footer
            await writer.WriteLineAsync("</body>");
            await writer.WriteLineAsync("</html>");
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

        private static string EscapeHtml(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}