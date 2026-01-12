namespace CoreLedger.Infrastructure.Configuration;

/// <summary>
///     RabbitMQ configuration options
/// </summary>
public class RabbitMQOptions
{
    /// <summary>
    ///     RabbitMQ hostname
    /// </summary>
    public string Hostname { get; set; } = "localhost";

    /// <summary>
    ///     RabbitMQ port
    /// </summary>
    public string Port { get; set; } = "5672";

    /// <summary>
    ///     RabbitMQ username
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    ///     RabbitMQ password
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    ///     Prefetch size for message consumption (0 = no limit)
    /// </summary>
    public uint PrefetchSize { get; set; } = 0;

    /// <summary>
    ///     Number of messages to prefetch
    /// </summary>
    public ushort PrefetchCount { get; set; } = 1;

    /// <summary>
    ///     Whether queues should survive broker restarts
    /// </summary>
    public bool QueueDurable { get; set; } = true;

    /// <summary>
    ///     Whether queues are used by only one connection
    /// </summary>
    public bool QueueExclusive { get; set; } = false;

    /// <summary>
    ///     Whether queues are deleted when last consumer unsubscribes
    /// </summary>
    public bool QueueAutoDelete { get; set; } = false;
}