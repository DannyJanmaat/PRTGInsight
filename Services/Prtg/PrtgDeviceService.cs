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
    public class PrtgDeviceService : BasePrtgService
    {
        public static async Task<List<PrtgDevice>> GetDevicesAsync(string serverUrl, string apiKey, CancellationToken _)
        {
            try
            {
                Debug.WriteLine("Getting devices with API key");

                // Create a temporary PrtgClient with API key treated as password
                PrtgClient client = new(serverUrl, "apitoken", apiKey, AuthMode.PassHash, false);

                // Get all devices
                List<Device> devices = await client.GetDevicesAsync(_);

                // Convert to our model using collection initializer
                var result = devices.Select(d => new PrtgDevice
                {
                    Id = d.Id,
                    Name = d.Name,
                    Status = d.Status.ToString(),
                    Type = d.Type.ToString(),
                    HostName = d.Host,
                    ParentId = d.ParentId,
                    Priority = (int)d.Priority,
                    IsActive = d.Active,
                    Message = d.Message,
                    Tags = string.Join(", ", d.Tags)
                }).ToList();

                Debug.WriteLine($"Retrieved {result.Count} devices");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting devices: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<PrtgDevice>> GetDevicesWithCredentialsAsync(string serverUrl, string username, string password, CancellationToken _)
        {
            try
            {
                Debug.WriteLine("Getting devices with credentials");

                // Create a temporary PrtgClient
                PrtgClient client = new(serverUrl, username, password, AuthMode.PassHash, false);

                // Get all devices
                List<Device> devices = await client.GetDevicesAsync(_);

                // Convert to our model using collection initializer
                var result = devices.Select(d => new PrtgDevice
                {
                    Id = d.Id,
                    Name = d.Name,
                    Status = d.Status.ToString(),
                    Type = d.Type.ToString(),
                    HostName = d.Host,
                    ParentId = d.ParentId,
                    Priority = (int)d.Priority,
                    IsActive = d.Active,
                    Message = d.Message,
                    Tags = string.Join(", ", d.Tags)
                }).ToList();

                Debug.WriteLine($"Retrieved {result.Count} devices");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting devices: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<PrtgDevice>> GetDevicesAsync(ConnectionInfo connectionInfo, CancellationToken _ = default)
        {
            try
            {
                Debug.WriteLine("Getting devices");

                PrtgClient client = GetPrtgClient(connectionInfo);

                // Get all devices
                List<Device> devices = await client.GetDevicesAsync(_);

                // Convert to our model using collection initializer
                var result = devices.Select(d => new PrtgDevice
                {
                    Id = d.Id,
                    Name = d.Name,
                    Status = d.Status.ToString(),
                    Type = d.Type.ToString(),
                    HostName = d.Host,
                    ParentId = d.ParentId,
                    Priority = (int)d.Priority,
                    IsActive = d.Active,
                    Message = d.Message,
                    Tags = string.Join(", ", d.Tags)
                }).ToList();

                Debug.WriteLine($"Retrieved {result.Count} devices");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting devices: {ex.Message}");
                throw;
            }
        }
    }
}
