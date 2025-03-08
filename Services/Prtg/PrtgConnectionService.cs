using PrtgAPI;
using PRTGInsight.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PRTGInsight.Services.Prtg
{
    public class PrtgConnectionService : BasePrtgService
    {
        public static async Task<PrtgStatus> TestConnectionWithApiKeyAsync(string serverUrl, string apiKey, CancellationToken _)
        {
            try
            {
                Debug.WriteLine($"Testing connection with API key to {serverUrl}");

                // Create a temporary PrtgClient
                var client = new PrtgClient(serverUrl, apiKey, AuthMode.PassHash.ToString());

                // Test connection by getting status
                var status = await client.GetStatusAsync(_);
                string version = status.Version.ToString();

                Debug.WriteLine($"Connection successful, PRTG version: {version}");
                return new PrtgStatus
                {
                    IsConnected = true,
                    Version = version,
                    Message = "Connection successful"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Connection error: {ex.Message}");
                return new PrtgStatus
                {
                    IsConnected = false,
                    Message = $"Connection error: {ex.Message}"
                };
            }
        }

        public static async Task<PrtgStatus> TestConnectionAsync(string serverUrl, string username, string password, CancellationToken _)
        {
            try
            {
                Debug.WriteLine($"Testing connection to {serverUrl} with username/password");

                // Create a temporary PrtgClient
                var client = new PrtgClient(serverUrl, username, password);

                // Test connection by getting status
                var status = await client.GetStatusAsync(_);
                string version = status.Version.ToString();

                Debug.WriteLine($"Connection successful, PRTG version: {version}");
                return new PrtgStatus
                {
                    IsConnected = true,
                    Version = version,
                    Message = "Connection successful"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Connection error: {ex.Message}");
                return new PrtgStatus
                {
                    IsConnected = false,
                    Message = $"Connection error: {ex.Message}"
                };
            }
        }
    }
}
