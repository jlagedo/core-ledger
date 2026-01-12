using System.Text;
using System.Text.Json;
using CoreLedger.Application.Constants;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using CoreLedger.Application.Interfaces;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;

namespace CoreLedger.Worker.Services;

/// <summary>
///     Background service that consumes test connection messages from RabbitMQ.
///     This consumer is used to test the API -> Queue -> Worker flow.
/// </summary>
public class TestConnectionConsumer : BackgroundService, IAsyncDisposable
{
    private readonly ILogger<TestConnectionConsumer> _logger;
    private readonly RabbitMQOptions _rabbitMQOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly TestConnectionOptions _testConnectionOptions;
    private IChannel? _channel;
    private IConnection? _connection;

    public TestConnectionConsumer(
        ILogger<TestConnectionConsumer> logger,
        IServiceProvider serviceProvider,
        IOptions<RabbitMQOptions> rabbitMQOptions,
        IOptions<TestConnectionOptions> testConnectionOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _rabbitMQOptions = rabbitMQOptions.Value;
        _testConnectionOptions = testConnectionOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TestConnectionConsumer starting");

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
                QueueNames.TestConnection,
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
                    _logger.LogInformation("========================================");
                    _logger.LogInformation("TEST CONNECTION MESSAGE RECEIVED");
                    _logger.LogInformation("========================================");
                    _logger.LogInformation("Queue: {QueueNames.TestConnection}", QueueNames.TestConnection);
                    _logger.LogInformation("Correlation ID: {CorrelationId}", correlationId ?? "none");
                    _logger.LogInformation("Message: {Message}", messageJson);

                    try
                    {
                        var message = JsonSerializer.Deserialize<TestConnectionMessage>(messageJson);
                        if (message == null)
                        {
                            _logger.LogError("Failed to deserialize test message: {MessageJson}", messageJson);
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                            return;
                        }

                        _logger.LogInformation("Deserialized Message:");
                        _logger.LogInformation("  - CoreJobId: {CoreJobId}", message.CoreJobId);
                        _logger.LogInformation("  - ReferenceId: {ReferenceId}", message.ReferenceId);
                        _logger.LogInformation("  - CommandType: {CommandType}", message.CommandType);
                        _logger.LogInformation("  - CorrelationId: {CorrelationId}", message.CorrelationId ?? "none");

                        // Update CoreJob status to Running
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                        var coreJob = await context.CoreJobs.FindAsync([message.CoreJobId], stoppingToken);
                        if (coreJob == null)
                        {
                            _logger.LogError("CoreJob not found with Id: {CoreJobId}", message.CoreJobId);
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                            return;
                        }

                        _logger.LogInformation("CoreJob found - Id: {CoreJobId}, Current Status: {Status}",
                            coreJob.Id, coreJob.Status);

                        // Update status to Running
                        coreJob.UpdateStatus(JobStatus.Running, DateTime.UtcNow);
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("CoreJob status updated to Running");

                        // Simulate processing
                        _logger.LogInformation("Simulating processing for {DelayMs} milliseconds...",
                            _testConnectionOptions.SimulatedProcessingDelayMs);
                        await Task.Delay(_testConnectionOptions.SimulatedProcessingDelayMs, stoppingToken);

                        // Update status to Complete
                        coreJob.UpdateStatus(JobStatus.Complete, finishedDate: DateTime.UtcNow);
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("CoreJob status updated to Complete");

                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

                        _logger.LogInformation("========================================");
                        _logger.LogInformation("TEST CONNECTION COMPLETED SUCCESSFULLY");
                        _logger.LogInformation("CoreJobId: {CoreJobId}, Final Status: {Status}", coreJob.Id,
                            coreJob.Status);
                        _logger.LogInformation("========================================");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing test message: {Message}", messageJson);
                        _logger.LogInformation("========================================");

                        // Update CoreJob status to Failed
                        try
                        {
                            var message = JsonSerializer.Deserialize<TestConnectionMessage>(messageJson);
                            if (message != null)
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                                var coreJob = await context.CoreJobs.FindAsync([message.CoreJobId], stoppingToken);
                                if (coreJob != null)
                                {
                                    coreJob.UpdateStatus(JobStatus.Failed, finishedDate: DateTime.UtcNow);
                                    await context.SaveChangesAsync(stoppingToken);
                                    _logger.LogInformation("CoreJob status updated to Failed");
                                }
                            }
                        }
                        catch (Exception updateEx)
                        {
                            _logger.LogError(updateEx, "Failed to update CoreJob status to Failed");
                        }

                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                    }
                }
            };

            await _channel.BasicConsumeAsync(QueueNames.TestConnection, false, consumer, stoppingToken);

            _logger.LogInformation("TestConnectionConsumer started and listening on queue: {QueueNames.TestConnection}",
                QueueNames.TestConnection);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TestConnectionConsumer");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_connection != null)
            await _connection.CloseAsync();
        _logger.LogInformation("TestConnectionConsumer disposed");
        GC.SuppressFinalize(this);
    }
}