namespace PRTGInsight.Models
{
    /// <summary>
    /// Represents the status of a PRTG connection.
    /// </summary>
    public class PrtgStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether a connection to the PRTG server is established.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets the version of the connected PRTG server.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a message associated with the connection status.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether there was an SSL certificate warning during the connection.
        /// </summary>
        public bool HasSslWarning { get; set; }
    }
}