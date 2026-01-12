using System.Net.Http.Json;
using System.Text;
using CoreLedger.Application.Events;
using CoreLedger.Application.UseCases.Transactions.Commands;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Worker.Configuration;
using MediatR;
using Microsoft.Extensions.Options;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;

namespace CoreLedger.Worker.Services;

/// <summary>
/// Background service that consumes transaction created events from RabbitMQ,
/// validates domain rules, updates transaction status, and notifies the API.
/// </summary>
public class TransactionProcessingConsumer(
    ILogger<TransactionProcessingConsumer> logger,
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    IOptions<RabbitMQOptions> rabbitMqOptions,
    IOptions<QueueNamesOptions> queueNames)
    : BackgroundService, IAsyncDisposable
{
    private readonly RabbitMQOptions _rabbitMQOptions = rabbitMqOptions.Value;
    private readonly QueueNamesOptions _queueNames = queueNames.Value;
    private IChannel? _channel;
    private IConnection? _connection;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TransactionProcessingConsumer starting");

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
                _queueNames.TransactionCreated,
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

                // Extract correlation ID from message headers or properties
                var correlationId = ea.BasicProperties?.CorrelationId;
                if (string.IsNullOrWhiteSpace(correlationId) && ea.BasicProperties?.Headers != null)
                    if (ea.BasicProperties.Headers.TryGetValue("X-Correlation-ID", out var headerValue) && headerValue is byte[] bytes)
                        correlationId = Encoding.UTF8.GetString(bytes);

                // Set up Serilog LogContext with correlation ID for distributed tracing
                using (LogContext.PushProperty("CorrelationId", correlationId ?? "unknown"))
                {
                    try
                    {
                        // 1. Deserialize Protobuf message
                        TransactionCreatedEvent transactionEvent;
                        using (var memoryStream = new MemoryStream(body))
                        {
                            transactionEvent = Serializer.Deserialize<TransactionCreatedEvent>(memoryStream);
                        }

                        logger.LogInformation(
                            "Transaction message received - TransactionId: {TransactionId}, " +
                            "FundCode: {FundCode}, Amount: {Amount} {Currency}",
                            transactionEvent.TransactionId,
                            transactionEvent.FundCode,
                            transactionEvent.Amount,
                            transactionEvent.Currency);

                        // 2. Process transaction via MediatR
                        using var scope = serviceProvider.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        var command = new ProcessTransactionCommand(
                            transactionEvent.TransactionId,
                            correlationId ?? "unknown");

                        var result = await mediator.Send(command, stoppingToken);

                        logger.LogInformation(
                            "Transaction processing completed - TransactionId: {TransactionId}, " +
                            "Success: {Success}, Status: {Status}",
                            result.TransactionId, result.Success, result.FinalStatusId);

                        // 3. HTTP callback to API (best-effort notification)
                        try
                        {
                            await NotifyApiAsync(result, correlationId ?? "unknown", stoppingToken);
                        }
                        catch (Exception notifyEx)
                        {
                            logger.LogWarning(notifyEx,
                                "Failed to notify API of transaction processing - TransactionId: {TransactionId}",
                                result.TransactionId);
                            // Don't fail the message - notification is best-effort
                        }

                        // 4. Acknowledge message
                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex,
                            "Error processing transaction message - Payload size: {PayloadSize} bytes",
                            body.Length);

                        // Requeue for retry on unexpected errors
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                    }
                }
            };

            await _channel.BasicConsumeAsync(_queueNames.TransactionCreated, false, consumer, stoppingToken);

            logger.LogInformation(
                "TransactionProcessingConsumer started and listening on queue: {QueueName}",
                _queueNames.TransactionCreated);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in TransactionProcessingConsumer");
            throw;
        }
    }

    /// <summary>
    /// Notifies the API of transaction processing completion via HTTP POST.
    /// </summary>
    private async Task NotifyApiAsync(
        ProcessTransactionResult result,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("WorkerHttpClient");

        var notification = new
        {
            TransactionId = result.TransactionId,
            Success = result.Success,
            FinalStatusId = result.FinalStatusId,
            ErrorMessage = result.ErrorMessage,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            CreatedByUserId = result.CreatedByUserId
        };

        var response = await httpClient.PostAsJsonAsync(
            "/api/worker-notifications/transaction-processed",
            notification,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        logger.LogInformation(
            "API notification sent successfully - TransactionId: {TransactionId}, StatusCode: {StatusCode}",
            result.TransactionId, response.StatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_connection != null)
            await _connection.CloseAsync();
        logger.LogInformation("TransactionProcessingConsumer disposed");
        GC.SuppressFinalize(this);
    }
}
