namespace CoreLedger.Application.Interfaces;

/// <summary>
///     Interface for publishing messages to message queues.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    ///     Publishes a message to the specified queue.
    /// </summary>
    /// <param name="queueName">Name of the queue to publish to</param>
    /// <param name="message">Message object to publish</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(string queueName, T message, string? correlationId = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Publishes a raw binary payload to the specified queue.
    ///     Used for pre-serialized messages like Protobuf payloads.
    /// </summary>
    /// <param name="queueName">Name of the queue to publish to</param>
    /// <param name="payload">Raw binary payload to publish</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing</param>
    /// <param name="contentType">Content type of the payload (default: application/protobuf)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishRawAsync(string queueName, byte[] payload, string? correlationId = null,
        string contentType = "application/protobuf", CancellationToken cancellationToken = default);
}