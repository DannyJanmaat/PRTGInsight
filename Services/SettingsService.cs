using Newtonsoft.Json;
using PRTGInsight.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PRTGInsight.Services
{
    public static class SettingsService
    {
        private static readonly string SettingsFileName = "settings.json";
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PRTGInsight");

        static SettingsService()
        {
            // Ensure the app data folder exists
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
        }

        private static string GetSettingsFilePath()
        {
            return Path.Combine(AppDataFolder, SettingsFileName);
        }

        public static ConnectionInfo LoadConnectionInfo()
        {
            try
            {
                string filePath = GetSettingsFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<ConnectionInfo>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading connection info: {ex.Message}");
                return null; // Return null instead of throwing to avoid app crashes
            }
        }

        public static async Task<ConnectionInfo> LoadConnectionInfoAsync()
        {
            try
            {
                string filePath = GetSettingsFilePath();
                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<ConnectionInfo>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading connection info asynchronously: {ex.Message}");
                return null; // Return null instead of throwing to avoid app crashes
            }
        }

        public static async Task SaveConnectionInfoAsync(ConnectionInfo connectionInfo)
        {
            try
            {
                string json = JsonConvert.SerializeObject(connectionInfo);
                string filePath = GetSettingsFilePath();
                await File.WriteAllTextAsync(filePath, json);
                Debug.WriteLine("Connection info saved successfully to file");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving connection info: {ex.Message}");
                // Don't throw to avoid app crashes
            }
        }

        internal static void ClearConnectionInfo()
        {
            try
            {
                string filePath = GetSettingsFilePath();
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing connection info: {ex.Message}");
                // Don't throw to avoid app crashes
            }
        }
    }
}