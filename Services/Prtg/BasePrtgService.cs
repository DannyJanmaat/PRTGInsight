using PrtgAPI;
using PRTGInsight.Models;
using System;
using System.Diagnostics;

namespace PRTGInsight.Services.Prtg
{
    public abstract class BasePrtgService
    {
        protected static PrtgClient GetPrtgClient(ConnectionInfo connectionInfo)
        {
            try
            {
                // Removed custom HttpClientHandler configuration because PrtgClient does not accept a handler parameter.
                if (connectionInfo.UseApiKey)
                {
                    return new PrtgClient(
                        connectionInfo.ServerUrl,
                        "apitoken", // Username
                        connectionInfo.ApiKey, // Password
                        AuthMode.PassHash,
                        connectionInfo.IgnoreSslErrors);
                }
                else
                {
                    return new PrtgClient(
                        connectionInfo.ServerUrl,
                        connectionInfo.Username,
                        connectionInfo.Password,
                        AuthMode.PassHash,
                        connectionInfo.IgnoreSslErrors);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating PrtgClient: {ex.Message}");
                throw;
            }
        }
    }
}
