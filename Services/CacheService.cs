using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace PRTGInsight.Services
{
    public static class CacheService
    {
        private static readonly string CacheFolderName = "Cache";

        public static async Task ClearCacheAsync()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder cacheFolder = await localFolder.CreateFolderAsync(
                    CacheFolderName, CreationCollisionOption.OpenIfExists);

                IReadOnlyList<StorageFile> files = await cacheFolder.GetFilesAsync();
                foreach (StorageFile file in files)
                {
                    await file.DeleteAsync();
                }

                // Also clear memory cache
                MemoryCache.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing cache: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> ReadCacheAsync(string key)
        {
            // First check memory cache
            if (MemoryCache.TryGetValue(key, out string memoryValue))
            {
                return memoryValue;
            }

            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder cacheFolder = await localFolder.CreateFolderAsync(
                    CacheFolderName, CreationCollisionOption.OpenIfExists);

                // Use a hash of the key as filename to avoid invalid characters
                string fileName = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(key)).Replace('/', '_').Replace('+', '-');

                try
                {
                    StorageFile file = await cacheFolder.GetFileAsync(fileName);
                    string content = await FileIO.ReadTextAsync(file);

                    // Also store in memory cache
                    MemoryCache.Set(key, content);

                    return content;
                }
                catch (System.IO.FileNotFoundException)
                {
                    // File doesn't exist, return null
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading from cache: {ex.Message}");
                return null;
            }
        }

        public static async Task WriteCacheAsync(string key, string content, TimeSpan? expiration = null)
        {
            try
            {
                // Store in memory cache
                MemoryCache.Set(key, content, expiration);

                // Also store in file cache
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder cacheFolder = await localFolder.CreateFolderAsync(
                    CacheFolderName, CreationCollisionOption.OpenIfExists);

                // Use a hash of the key as filename to avoid invalid characters
                string fileName = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(key)).Replace('/', '_').Replace('+', '-');

                StorageFile file = await cacheFolder.CreateFileAsync(
                    fileName, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(file, content);

                // If expiration is set, store it in a separate metadata file
                if (expiration.HasValue)
                {
                    StorageFile metaFile = await cacheFolder.CreateFileAsync(
                        fileName + ".meta", CreationCollisionOption.ReplaceExisting);

                    DateTime expirationTime = DateTime.Now.Add(expiration.Value);
                    await FileIO.WriteTextAsync(metaFile, expirationTime.ToString("o"));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing to cache: {ex.Message}");
            }
        }

        public static async Task<bool> IsCacheExpiredAsync(string key)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder cacheFolder = await localFolder.CreateFolderAsync(
                    CacheFolderName, CreationCollisionOption.OpenIfExists);

                // Use a hash of the key as filename to avoid invalid characters
                string fileName = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(key)).Replace('/', '_').Replace('+', '-');

                try
                {
                    // Check if metadata file exists
                    StorageFile metaFile = await cacheFolder.GetFileAsync(fileName + ".meta");
                    string expirationTimeStr = await FileIO.ReadTextAsync(metaFile);

                    if (DateTime.TryParse(expirationTimeStr, out DateTime expirationTime))
                    {
                        return DateTime.Now > expirationTime;
                    }

                    return false; // No valid expiration time, assume not expired
                }
                catch (System.IO.FileNotFoundException)
                {
                    // No metadata file, assume not expired
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking cache expiration: {ex.Message}");
                return true; // On error, assume expired to be safe
            }
        }
    }

    // Simple in-memory cache
    public static class MemoryCache
    {
        private static readonly Dictionary<string, CacheItem> _cache = [];

        public static void Set(string key, string value, TimeSpan? expiration = null)
        {
            DateTime? expirationTime = expiration.HasValue ?
                DateTime.Now.Add(expiration.Value) : null;

            _cache[key] = new CacheItem
            {
                Value = value,
                ExpirationTime = expirationTime
            };
        }

        public static bool TryGetValue(string key, out string value)
        {
            if (_cache.TryGetValue(key, out CacheItem item))
            {
                // Check if expired
                if (item.ExpirationTime.HasValue && DateTime.Now > item.ExpirationTime.Value)
                {
                    _ = _cache.Remove(key);
                    value = null;
                    return false;
                }

                value = item.Value;
                return true;
            }

            value = null;
            return false;
        }

        public static void Remove(string key)
        {
            _ = _cache.Remove(key);
        }

        public static void Clear()
        {
            _cache.Clear();
        }

        private class CacheItem
        {
            public string Value { get; set; }
            public DateTime? ExpirationTime { get; set; }
        }
    }
}
