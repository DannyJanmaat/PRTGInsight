using PrtgAPI;
using PRTGInsight.Models;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PRTGInsight.Services.Prtg
{
    public abstract class BasePrtgService
    {
        protected static PrtgClient GetPrtgClient(ConnectionInfo connectionInfo)
        {
            try
            {
                Debug.WriteLine($"[DEBUG] Creating PrtgClient for {connectionInfo.ServerUrl}, UseApiKey={connectionInfo.UseApiKey}");

                // Format URL properly
                string serverUrl = EnsureProtocol(connectionInfo.ServerUrl);

                // Configure SSL for the application
                ConfigureSSLValidation();

                // Create client based on connection type
                if (connectionInfo.UseApiKey)
                {
                    // For API token auth - use the method that works in the logs: apitoken with direct URL parameter
                    // First try with the token as parameter approach that was successful in logs
                    try
                    {
                        Debug.WriteLine("[DEBUG] Creating PrtgClient with API token (direct token approach)");
                        // PrtgAPI's standard approach for API tokens
                        return new PrtgClient(serverUrl, "apitoken", connectionInfo.ApiKey, AuthMode.PassHash, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DEBUG] Standard API token approach failed: {ex.Message}, trying alternate");

                        // Fall back to using token as username
                        return new PrtgClient(serverUrl, connectionInfo.ApiKey, "", AuthMode.Password, true);
                    }
                }
                else
                {
                    // For username/password auth, first try with Password mode (which is more common)
                    try
                    {
                        Debug.WriteLine("[DEBUG] Creating PrtgClient with username/password (Password mode)");
                        return new PrtgClient(
                            serverUrl,
                            connectionInfo.Username,
                            connectionInfo.Password,
                            AuthMode.Password,
                            connectionInfo.IgnoreSslErrors);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DEBUG] Password mode failed: {ex.Message}, trying PassHash mode");

                        // Fall back to PassHash mode
                        return new PrtgClient(
                            serverUrl,
                            connectionInfo.Username,
                            connectionInfo.Password,
                            AuthMode.PassHash,
                            connectionInfo.IgnoreSslErrors);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error creating PrtgClient: {ex.Message}");
                throw;
            }
        }

        private static string EnsureProtocol(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            url = url.Trim();

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + url;
            }

            // Remove trailing slash for consistency
            return url.TrimEnd('/');
        }

        private static void ConfigureSSLValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    if (sslPolicyErrors != SslPolicyErrors.None)
                    {
                        Debug.WriteLine($"[WARNING] SSL Certificate validation error: {sslPolicyErrors}");
                    }
                    return true; // Accept all certificates
                };

            // Ensure all TLS versions are supported
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                  SecurityProtocolType.Tls11 |
                                                  SecurityProtocolType.Tls;
        }
    }
}