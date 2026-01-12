using System.Text;
using System.Text.Json;
using CoreLedger.Application.Constants;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;

namespace CoreLedger.Worker.Services;

/// <summary>
///     Background service that consumes B3 import messages from RabbitMQ.
/// </summary>
public class B3ImportConsumer : BackgroundService, IAsyncDisposable
{
    private readonly ILogger<B3ImportConsumer> _logger;
    private readonly RabbitMQOptions _rabbitMQOptions;
    private readonly IServiceProvider _serviceProvider;
    private IChannel? _channel;
    private IConnection? _connection;

    public B3ImportConsumer(
        ILogger<B3ImportConsumer> logger,
        IServiceProvider serviceProvider,
        IOptions<RabbitMQOptions> rabbitMQOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _rabbitMQOptions = rabbitMQOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("B3ImportConsumer starting");

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMQOptions.Hostname,
            Port = int.Parse(_rabbitMQOptions.Port),
            UserName = _rabbitMQOptions.Username,
            Password = _rabbitMQOptions.Password
        };

        try
        {
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                QueueNames.B3Import,
                _rabbitMQOptions.QueueDurable,
                _rabbitMQOptions.QueueExclusive,
                _rabbitMQOptions.QueueAutoDelete,
                null,
                cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(
                _rabbitMQOptions.PrefetchSize,
                _rabbitMQOptions.PrefetchCount,
                false,
                stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                // Extract correlation ID from message headers or properties
                var correlationId = ea.BasicProperties?.CorrelationId;
                if (string.IsNullOrWhiteSpace(correlationId) && ea.BasicProperties?.Headers != null)
                    if (ea.BasicProperties.Headers.TryGetValue("X-Correlation-ID", out var headerValue) && headerValue is byte[] bytes)
                        correlationId = Encoding.UTF8.GetString(bytes);

                // Set up Serilog LogContext with correlation ID for distributed tracing
                using (LogContext.PushProperty("CorrelationId", correlationId ?? "unknown"))
                {
                    _logger.LogInformation("Received message from {QueueName}: {Message}", QueueNames.B3Import,
                        messageJson);

                    try
                    {
                        var message = JsonSerializer.Deserialize<CoreJobB3ImportMessage>(messageJson);
                        if (message == null)
                        {
                            _logger.LogError("Failed to deserialize message: {MessageJson}", messageJson);
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                            return;
                        }

                        if (message.CommandType != "CoreJobB3Import")
                        {
                            _logger.LogWarning("Unexpected command type: {CommandType}", message.CommandType);
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                            return;
                        }

                        using var scope = _serviceProvider.CreateScope();
                        var processor = scope.ServiceProvider.GetRequiredService<IB3ImportProcessor>();

                        await processor.ProcessAsync(message.CoreJobId, message.ReferenceId, stoppingToken);

                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        _logger.LogInformation("Successfully processed B3 import for CoreJob {CoreJobId}",
                            message.CoreJobId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message: {Message}", messageJson);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                    }
                }
            };

            await _channel.BasicConsumeAsync(QueueNames.B3Import, false, consumer, stoppingToken);

            _logger.LogInformation("B3ImportConsumer started and listening on queue: {QueueName}", QueueNames.B3Import);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in B3ImportConsumer");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_connection != null)
            await _connection.CloseAsync();
        _logger.LogInformation("B3ImportConsumer disposed");
        GC.SuppressFinalize(this);
    }
}