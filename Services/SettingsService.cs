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
        private static readonly string AppSettingsFileName = "appsettings.json";
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PRTGInsight");

        static SettingsService()
        {
            // Ensure the app data folder exists
            if (!Directory.Exists(AppDataFolder))
            {
                _ = Directory.CreateDirectory(AppDataFolder);
            }
        }

        private static string GetSettingsFilePath()
        {
            return Path.Combine(AppDataFolder, SettingsFileName);
        }

        private static string GetAppSettingsFilePath()
        {
            return Path.Combine(AppDataFolder, AppSettingsFileName);
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

        /// <summary>
        /// Gets a setting value from the application settings.
        /// </summary>
        /// <typeparam name="T">The type of the setting value</typeparam>
        /// <param name="key">The setting key</param>
        /// <param name="defaultValue">The default value if the setting is not found</param>
        /// <returns>The setting value or the default value if not found</returns>
        public static T GetSetting<T>(string key, T defaultValue)
        {
            try
            {
                string filePath = GetAppSettingsFilePath();
                if (!File.Exists(filePath))
                {
                    return defaultValue;
                }

                string json = File.ReadAllText(filePath);
                System.Collections.Generic.Dictionary<string, object> settings = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(json);

                if (settings != null && settings.TryGetValue(key, out object value))
                {
                    try
                    {
                        // Try to convert the value to the target type
                        return value is Newtonsoft.Json.Linq.JToken token ? token.ToObject<T>() : (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        // If conversion fails, return the default value
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting setting {key}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Saves a setting value to the application settings.
        /// </summary>
        /// <typeparam name="T">The type of the setting value</typeparam>
        /// <param name="key">The setting key</param>
        /// <param name="value">The setting value</param>
        public static void SaveSetting<T>(string key, T value)
        {
            try
            {
                string filePath = GetAppSettingsFilePath();
                System.Collections.Generic.Dictionary<string, object> settings;

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    settings = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(json)
                        ?? [];
                }
                else
                {
                    settings = [];
                }

                // Update or add the setting
                settings[key] = value;

                // Save the settings
                string updatedJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, updatedJson);

                Debug.WriteLine($"Saved setting {key}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving setting {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a setting from the application settings.
        /// </summary>
        /// <param name="key">The setting key</param>
        public static void RemoveSetting(string key)
        {
            try
            {
                string filePath = GetAppSettingsFilePath();
                if (!File.Exists(filePath))
                {
                    return;
                }

                string json = File.ReadAllText(filePath);
                System.Collections.Generic.Dictionary<string, object> settings = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(json);

                if (settings != null && settings.ContainsKey(key))
                {
                    _ = settings.Remove(key);

                    // Save the settings
                    string updatedJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(filePath, updatedJson);

                    Debug.WriteLine($"Removed setting {key}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing setting {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all application settings.
        /// </summary>
        public static void ClearAllSettings()
        {
            try
            {
                string filePath = GetAppSettingsFilePath();
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.WriteLine("Cleared all settings");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing settings: {ex.Message}");
            }
        }
    }
}