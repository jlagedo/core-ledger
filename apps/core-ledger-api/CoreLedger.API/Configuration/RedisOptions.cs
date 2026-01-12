namespace CoreLedger.API.Configuration;

/// <summary>
/// Redis configuration options for SignalR backplane.
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Redis connection string (e.g., "localhost:6379").
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Channel prefix for SignalR messages to isolate from other applications.
    /// </summary>
    public string ChannelPrefix { get; set; } = "CoreLedger";
}
