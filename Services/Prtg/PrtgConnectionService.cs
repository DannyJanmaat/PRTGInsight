using PrtgAPI;
using PRTGInsight.Models;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace PRTGInsight.Services.Prtg
{
    public class PrtgConnectionService : BasePrtgService
    {
        // PRTG version-specific configuration for v23.1
        private const string PRTG_VERSION = "23.1.82.2074";
        private const string API_TOKEN_USERNAME = "apitoken";

        public static async Task<PrtgStatus> TestConnectionWithApiKeyAsync(string serverUrl, string apiKey, CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine($"[DEBUG] Testing connection with API key to {serverUrl} for PRTG v{PRTG_VERSION}");

                // Ensure URL has protocol and remove trailing slash
                serverUrl = EnsureProtocol(serverUrl);
                Debug.WriteLine($"[DEBUG] Formatted server URL: {serverUrl}");

                // Configure SSL certificate validation to accept all certificates
                ConfigureSSLValidation();

                // Sanitize the API key - remove any whitespace
                string cleanApiKey = apiKey?.Trim();

                if (string.IsNullOrWhiteSpace(cleanApiKey))
                {
                    Debug.WriteLine("[ERROR] API key is empty or whitespace");
                    return new PrtgStatus
                    {
                        IsConnected = false,
                        Message = "API key cannot be empty"
                    };
                }

                Debug.WriteLine($"[DEBUG] Using API key authentication (length: {cleanApiKey.Length})");

                // Try multiple authentication approaches
                try
                {
                    // First try: Standard apitoken approach
                    Debug.WriteLine("[ATTEMPT] Using standard API token authentication");
                    PrtgClient client = new(serverUrl, API_TOKEN_USERNAME, cleanApiKey, AuthMode.PassHash, true);

                    ServerStatus status = await client.GetStatusAsync(cancellationToken);
                    string version = status.Version.ToString();

                    Debug.WriteLine($"[SUCCESS] Connection successful with API token, PRTG version: {version}");
                    return new PrtgStatus
                    {
                        IsConnected = true,
                        Version = version,
                        Message = "Connection successful"
                    };
                }
                catch (Exception ex1)
                {
                    Debug.WriteLine($"[FAIL] Standard API token authentication failed: {ex1.Message}");

                    // Second try: With prtgadmin username
                    try
                    {
                        Debug.WriteLine("[ATTEMPT] Using prtgadmin with API key as password");
                        PrtgClient client = new(serverUrl, "prtgadmin", cleanApiKey, AuthMode.Password, true);

                        ServerStatus status = await client.GetStatusAsync(cancellationToken);
                        string version = status.Version.ToString();

                        Debug.WriteLine($"[SUCCESS] Connection successful with alternate method, PRTG version: {version}");
                        return new PrtgStatus
                        {
                            IsConnected = true,
                            Version = version,
                            Message = "Connection successful"
                        };
                    }
                    catch (Exception ex2)
                    {
                        Debug.WriteLine($"[FAIL] Alternate authentication failed: {ex2.Message}");

                        // Last resort: Try direct HTTP requests to different endpoints
                        return await TryDirectApiRequests(serverUrl, cleanApiKey, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Unexpected error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
                }

                return new PrtgStatus
                {
                    IsConnected = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        private static async Task<PrtgStatus> TryDirectApiRequests(string serverUrl, string apiKey, CancellationToken cancellationToken)
        {
            Debug.WriteLine("[DEBUG] Attempting direct API connections...");

            // Define multiple endpoint formats to try
            var endpoints = new[]
            {
                $"/api/getstatus.xml?username={API_TOKEN_USERNAME}&passhash={Uri.EscapeDataString(apiKey)}",
                $"/api/table.json?content=status&output=json&username={API_TOKEN_USERNAME}&passhash={Uri.EscapeDataString(apiKey)}",
                $"/api/table.json?content=status&username=prtgadmin&passhash={Uri.EscapeDataString(apiKey)}",
                $"/api/table.json?apitoken={Uri.EscapeDataString(apiKey)}"
            };

            foreach (string endpoint in endpoints)
            {
                try
                {
                    string apiUrl = serverUrl + endpoint;

                    // Create a safe URL for logging (hide the actual API token)
                    string safeUrl = apiUrl;
                    int tokenIndex = apiUrl.IndexOf("passhash=");
                    if (tokenIndex >= 0)
                    {
                        safeUrl = apiUrl[..(tokenIndex + 9)] + "[TOKEN-HIDDEN]";
                    }
                    else if (apiUrl.Contains("apitoken="))
                    {
                        tokenIndex = apiUrl.IndexOf("apitoken=");
                        safeUrl = apiUrl[..(tokenIndex + 9)] + "[TOKEN-HIDDEN]";
                    }

                    Debug.WriteLine($"[DEBUG] Trying endpoint: {safeUrl}");

                    using HttpClientHandler handler = new()
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
                        AllowAutoRedirect = true
                    };

                    using HttpClient client = new(handler) { Timeout = TimeSpan.FromSeconds(15) };
                    client.DefaultRequestHeaders.Add("User-Agent", "PRTGInsight");

                    // Make the request
                    using HttpResponseMessage response = await client.GetAsync(apiUrl, cancellationToken);
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);

                    Debug.WriteLine($"[DEBUG] Response: {response.StatusCode}, Content Length: {content?.Length ?? 0}");

                    // Check if response indicates success
                    if (response.IsSuccessStatusCode &&
                        (content.Contains("prtg-version") || content.Contains("PRTG") ||
                         (!content.Contains("<error>") && !content.Contains("\"error\""))))
                    {
                        Debug.WriteLine("[SUCCESS] Direct API connection successful");
                        return new PrtgStatus
                        {
                            IsConnected = true,
                            Version = PRTG_VERSION,
                            Message = "Connected via direct API"
                        };
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] Endpoint failed: {ex.Message}");
                    // Continue to next endpoint
                }
            }

            Debug.WriteLine("[FAIL] All API connection attempts failed");
            return new PrtgStatus
            {
                IsConnected = false,
                Message = "Could not connect with any method. Please verify your API key and server URL."
            };
        }

        public static async Task<PrtgStatus> TestConnectionAsync(string serverUrl, string username, string password, CancellationToken cancellationToken)
        {
            try
            {
                // Ensure URL has protocol and remove trailing slash
                serverUrl = EnsureProtocol(serverUrl);
                Debug.WriteLine($"[DEBUG] Formatted server URL: {serverUrl}");
                Debug.WriteLine($"[DEBUG] Testing connection to {serverUrl} with username/password");

                // Configure SSL validation to accept all certificates
                ConfigureSSLValidation();
                bool hasSslWarning = HasSslCertificateWarning(serverUrl);

                // Try Password authentication mode first
                try
                {
                    Debug.WriteLine("[ATTEMPT] Using Password authentication mode");
                    PrtgClient client = new(serverUrl, username, password, AuthMode.Password, true);

                    ServerStatus status = await client.GetStatusAsync(cancellationToken);
                    string version = status.Version.ToString();

                    Debug.WriteLine($"[SUCCESS] Connection successful with Password mode, PRTG version: {version}");
                    return new PrtgStatus
                    {
                        IsConnected = true,
                        Version = version,
                        Message = "Connection successful",
                        HasSslWarning = hasSslWarning
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FAIL] Password authentication failed: {ex.Message}");

                    // Try PassHash authentication mode
                    try
                    {
                        Debug.WriteLine("[ATTEMPT] Using PassHash authentication mode");
                        PrtgClient client = new(serverUrl, username, password, AuthMode.PassHash, true);

                        ServerStatus status = await client.GetStatusAsync(cancellationToken);
                        string version = status.Version.ToString();

                        Debug.WriteLine($"[SUCCESS] Connection successful with PassHash mode, PRTG version: {version}");
                        return new PrtgStatus
                        {
                            IsConnected = true,
                            Version = version,
                            Message = "Connection successful",
                            HasSslWarning = hasSslWarning
                        };
                    }
                    catch (Exception ex2)
                    {
                        Debug.WriteLine($"[FAIL] PassHash authentication failed: {ex2.Message}");

                        // Try direct API request
                        try
                        {
                            string apiUrl = $"{serverUrl}/api/table.json?content=status&output=json&username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";

                            using HttpClientHandler handler = new() { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
                            using HttpClient client = new(handler) { Timeout = TimeSpan.FromSeconds(15) };

                            using HttpResponseMessage response = await client.GetAsync(apiUrl, cancellationToken);
                            string content = await response.Content.ReadAsStringAsync(cancellationToken);

                            if (response.IsSuccessStatusCode && !content.Contains("error"))
                            {
                                Debug.WriteLine("[SUCCESS] Direct API login successful");
                                return new PrtgStatus
                                {
                                    IsConnected = true,
                                    Version = PRTG_VERSION,
                                    Message = "Connected via direct API",
                                    HasSslWarning = hasSslWarning
                                };
                            }

                            throw new Exception($"API returned: {response.StatusCode}");
                        }
                        catch (Exception ex3)
                        {
                            Debug.WriteLine($"[FAIL] Direct API login failed: {ex3.Message}");
                            throw new Exception("All authentication methods failed", ex);
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERROR] HttpRequestException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
                }

                bool isSslError = ex.Message.Contains("SSL") || ex.Message.Contains("certificate") ||
                                 (ex.InnerException?.Message?.Contains("SSL") == true);

                return new PrtgStatus
                {
                    IsConnected = false,
                    Message = $"Connection error: {ex.Message}",
                    HasSslWarning = isSslError
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Unexpected error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
                }

                return new PrtgStatus
                {
                    IsConnected = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
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

        private static string EnsureProtocol(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            url = url.Trim();

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + url;
            }

            // Remove trailing slash for consistency
            return url.TrimEnd('/');
        }

        private static bool HasSslCertificateWarning(string url)
        {
            try
            {
                if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    RemoteCertificateValidationCallback originalCallback = ServicePointManager.ServerCertificateValidationCallback;
                    bool hasSslIssue = false;

                    ServicePointManager.ServerCertificateValidationCallback =
                        delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        {
                            if (sslPolicyErrors != SslPolicyErrors.None)
                            {
                                hasSslIssue = true;
                                Debug.WriteLine($"[WARNING] SSL validation issues: {sslPolicyErrors}");
                            }
                            return true;
                        };

                    using (HttpClientHandler handler = new()
                    { ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
                    using (HttpClient client = new(handler))
                    {
                        HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                        Debug.WriteLine($"[DEBUG] SSL check response: {response.StatusCode}");
                    }

                    ServicePointManager.ServerCertificateValidationCallback = originalCallback;
                    return hasSslIssue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error checking SSL certificate: {ex.Message}");
                return true; // Assume SSL issues if we couldn't check
            }

            return false;
        }
    }
}