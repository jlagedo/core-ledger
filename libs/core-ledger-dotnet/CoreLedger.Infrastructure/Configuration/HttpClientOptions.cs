namespace CoreLedger.Infrastructure.Configuration;

/// <summary>
///     HTTP client configuration options
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    ///     Timeout in seconds for Auth0 HTTP requests
    /// </summary>
    public int Auth0TimeoutSeconds { get; set; } = 30;

    /// <summary>
    ///     User-Agent header value for HTTP requests
    /// </summary>
    public string UserAgent { get; set; } = "CoreLedgerAPI/1.0";
}