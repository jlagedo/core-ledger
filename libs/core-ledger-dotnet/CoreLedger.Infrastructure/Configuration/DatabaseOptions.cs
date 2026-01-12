namespace CoreLedger.Infrastructure.Configuration;

/// <summary>
///     Database configuration options
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    ///     Maximum number of retry attempts for transient database failures
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    ///     Maximum delay in seconds between retry attempts
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 30;

    /// <summary>
    ///     Command timeout in seconds
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;
}