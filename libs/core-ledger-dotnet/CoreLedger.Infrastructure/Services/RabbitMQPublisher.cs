using System.Text;
using System.Text.Json;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
///     RabbitMQ implementation of the message publisher.
/// </summary>
public class RabbitMQPublisher : IMessagePublisher, IAsyncDisposable
{
    private IChannel? _channel;
    private IConnection? _connection;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly RabbitMQOptions _options;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _disposed;
    private bool _initialized;

    public RabbitMQPublisher(IOptions<RabbitMQOptions> options, ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Throws ObjectDisposedException if the object has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQPublisher));
    }

    /// <summary>
    /// Lazy initialization of RabbitMQ connection and channel.
    /// </summary>
    private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_initialized)
            return;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_initialized)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _options.Hostname,
                Port = int.Parse(_options.Port),
                UserName = _options.Username,
                Password = _options.Password
            };

            try
            {
                _connection = await factory.CreateConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogInformation("RabbitMQ connection established to {Hostname}:{Port}",
                    _options.Hostname, _options.Port);

                _initialized = true;
            }
            catch
            {
                // Clean up partial initialization
                if (_channel != null)
                {
                    await _channel.CloseAsync().ConfigureAwait(false);
                    _channel = null;
                }

                if (_connection != null)
                {
                    await _connection.CloseAsync().ConfigureAwait(false);
                    _connection = null;
                }

                throw;
            }
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs asynchronous cleanup of resources.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        // Wait for any pending initialization to complete before disposing
        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;

            if (_channel is not null)
            {
                await _channel.CloseAsync().ConfigureAwait(false);
                _channel = null;
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync().ConfigureAwait(false);
                _connection = null;
            }

            _disposed = true;
            _logger.LogInformation("RabbitMQ connection disposed");
        }
        finally
        {
            _initLock.Release();
        }

        _initLock.Dispose();
    }

    /// <summary>
    ///     Publishes a message to the specified queue.
    /// </summary>
    public async Task PublishAsync<T>(string queueName, T message, string? correlationId = null,
        CancellationToken cancellationToken = default) where T : class
    {
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _channel!.QueueDeclareAsync(
                queueName,
                _options.QueueDurable,
                _options.QueueExclusive,
                _options.QueueAutoDelete,
                null,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            // Add correlation ID to message headers for distributed tracing
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                properties.CorrelationId = correlationId;
                properties.Headers ??= new Dictionary<string, object?>();
                properties.Headers["X-Correlation-ID"] = correlationId;
            }

            await _channel.BasicPublishAsync(
                string.Empty,
                queueName,
                false,
                properties,
                body,
                cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Message published to queue {QueueName} with CorrelationId {CorrelationId}",
                queueName, correlationId ?? "none");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish message to queue {QueueName} with CorrelationId {CorrelationId}. " +
                "Message type: {MessageType}",
                queueName, correlationId ?? "none", typeof(T).Name);
            throw new ExternalServiceException("RabbitMQ",
                $"Failed to publish message to queue {queueName}", ex);
        }
    }

    /// <summary>
    ///     Publishes a raw binary payload to the specified queue.
    /// </summary>
    public async Task PublishRawAsync(string queueName, byte[] payload, string? correlationId = null,
        string contentType = "application/protobuf", CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _channel!.QueueDeclareAsync(
                queueName,
                _options.QueueDurable,
                _options.QueueExclusive,
                _options.QueueAutoDelete,
                null,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = contentType
            };

            // Add correlation ID to message headers for distributed tracing
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                properties.CorrelationId = correlationId;
                properties.Headers ??= new Dictionary<string, object?>();
                properties.Headers["X-Correlation-ID"] = correlationId;
            }

            await _channel.BasicPublishAsync(
                string.Empty,
                queueName,
                false,
                properties,
                payload,
                cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Raw payload published to queue {QueueName} - Size: {PayloadSize} bytes, " +
                "ContentType: {ContentType}, CorrelationId: {CorrelationId}",
                queueName, payload.Length, contentType, correlationId ?? "none");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish raw payload to queue {QueueName} with CorrelationId {CorrelationId}. " +
                "ContentType: {ContentType}, PayloadSize: {PayloadSize} bytes",
                queueName, correlationId ?? "none", contentType, payload.Length);
            throw new ExternalServiceException("RabbitMQ",
                $"Failed to publish raw payload to queue {queueName}", ex);
        }
    }
}