using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PRTGInsight.Services
{
    public static class LoginHistoryService
    {
        private static readonly string HistoryFileName = "login_history.json";
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PRTGInsight");
        private static readonly int MaxHistoryItems = 10;

        static LoginHistoryService()
        {
            // Ensure the app data folder exists
            if (!Directory.Exists(AppDataFolder))
            {
                _ = Directory.CreateDirectory(AppDataFolder);
            }
        }

        private static string GetHistoryFilePath()
        {
            return Path.Combine(AppDataFolder, HistoryFileName);
        }

        public static async Task<List<string>> LoadLoginHistoryAsync()
        {
            try
            {
                string filePath = GetHistoryFilePath();
                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    return JsonSerializer.Deserialize<List<string>>(json) ?? [];
                }
                return [];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading login history: {ex.Message}");
                return [];
            }
        }

        public static async Task AddUsernameToHistoryAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            try
            {
                // Load existing history
                List<string> history = await LoadLoginHistoryAsync();

                // Remove the username if it already exists (to avoid duplicates)
                _ = history.Remove(username);

                // Add the username to the beginning of the list
                history.Insert(0, username);

                // Trim the list to the maximum number of items
                if (history.Count > MaxHistoryItems)
                {
                    history = [.. history.Take(MaxHistoryItems)];
                }

                // Save the updated history
                string json = JsonSerializer.Serialize(history);
                await File.WriteAllTextAsync(GetHistoryFilePath(), json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding username to history: {ex.Message}");
            }
        }
    }
}
