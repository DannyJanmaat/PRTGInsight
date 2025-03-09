using PRTGInsight.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PRTGInsight.Services
{
    public class DashboardExportService(ConnectionInfo connectionInfo)
    {
        private readonly ConnectionInfo _connectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));

        public async Task<string> ExportDashboardAsync()
        {
            // Create export directory if it doesn't exist
            string exportDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PRTGInsight",
                "Exports");

            _ = Directory.CreateDirectory(exportDir);

            // Create file name with timestamp
            string fileName = $"Dashboard_Export_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(exportDir, fileName);

            // Generate export content
            StringBuilder content = new();

            _ = content.AppendLine("PRTG DASHBOARD EXPORT");
            _ = content.AppendLine("====================");
            _ = content.AppendLine();
            _ = content.AppendLine($"Server: {_connectionInfo.ServerUrl}");
            _ = content.AppendLine($"Date: {DateTime.Now}");
            _ = content.AppendLine();

            // Add dashboard data - in a real implementation, you would get this from your service
            _ = content.AppendLine("SYSTEM STATUS");
            _ = content.AppendLine("-------------");
            _ = content.AppendLine("CPU Load: [Value]%");
            _ = content.AppendLine("Memory Usage: [Value]%");
            _ = content.AppendLine("Uptime: [Value]");
            _ = content.AppendLine();

            _ = content.AppendLine("SENSORS STATUS");
            _ = content.AppendLine("--------------");
            _ = content.AppendLine("Total Sensors: [Value]");
            _ = content.AppendLine("Up Sensors: [Value]");
            _ = content.AppendLine("Down Sensors: [Value]");
            _ = content.AppendLine("Warning Sensors: [Value]");
            _ = content.AppendLine("Paused Sensors: [Value]");
            _ = content.AppendLine();

            // Write to file
            await File.WriteAllTextAsync(filePath, content.ToString());

            return filePath;
        }
    }
}
