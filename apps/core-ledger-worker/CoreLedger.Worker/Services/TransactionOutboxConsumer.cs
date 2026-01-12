using CoreLedger.Application.Interfaces;
using CoreLedger.Application.UseCases.Transactions.Commands;
using CoreLedger.Worker.Configuration;
using CoreLedger.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace CoreLedger.Worker.Services;

/// <summary>
/// Background service that implements the Transactional Outbox Pattern.
/// Polls the transaction_created_outbox_message table for pending messages,
/// deserializes Protobuf payloads, and sends commands to the Application layer via MediatR.
/// </summary>
public class TransactionOutboxProcessor : BackgroundService
{
    private readonly ILogger<TransactionOutboxProcessor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly OutboxProcessorOptions _options;
    private readonly QueueNamesOptions _queueNames;

    public TransactionOutboxProcessor(
        ILogger<TransactionOutboxProcessor> logger,
        IServiceProvider serviceProvider,
        IOptions<OutboxProcessorOptions> options,
        IOptions<QueueNamesOptions> queueNames)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _queueNames = queueNames.Value;
    }

    /// <summary>
    /// Main execution loop that polls the outbox table at regular intervals.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "TransactionOutboxProcessor starting - PollingInterval: {PollingIntervalMs}ms, " +
            "BatchSize: {BatchSize}, MaxRetryCount: {MaxRetryCount}",
            _options.PollingIntervalMs, _options.BatchSize, _options.MaxRetryCount);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
                await Task.Delay(_options.PollingIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TransactionOutboxProcessor stopping gracefully");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in TransactionOutboxProcessor polling loop");
                // Continue processing - don't crash the worker on transient errors
                await Task.Delay(_options.PollingIntervalMs, stoppingToken);
            }
        }

        _logger.LogInformation("TransactionOutboxProcessor stopped");
    }

    /// <summary>
    /// Fetches a batch of pending outbox messages and processes each one.
    /// Uses raw SQL with FOR UPDATE SKIP LOCKED to enable horizontal scaling.
    /// </summary>
    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Raw SQL with FOR UPDATE SKIP LOCKED (enables horizontal scaling)
        // Fetches Pending (0) or Failed (2) messages that haven't exceeded retry limit
        var sql = @"
            SELECT *
            FROM transaction_created_outbox_message
            WHERE (status = 0)
               OR (status = 2 AND retry_count < {0})
            ORDER BY occurred_on
            FOR UPDATE SKIP LOCKED
            LIMIT {1}";

        var messages = await context.TransactionCreatedOutboxMessages
            .FromSqlRaw(sql, _options.MaxRetryCount, _options.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count > 0)
        {
            _logger.LogInformation("Fetched {MessageCount} outbox messages for processing", messages.Count);

            foreach (var message in messages)
            {
                await ProcessMessageAsync(message, context, mediator, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Processes a single outbox message by sending the raw Protobuf payload
    /// to the Application layer and updating the message status.
    /// </summary>
    private async Task ProcessMessageAsync(
        TransactionCreatedOutboxMessage message,
        IApplicationDbContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            // Set OutboxMessageId in LogContext for tracking
            using (LogContext.PushProperty("OutboxMessageId", message.Id))
            {
                _logger.LogInformation(
                    "Processing outbox message - MessageId: {MessageId}, " +
                    "RetryCount: {RetryCount}, PayloadSize: {PayloadSize} bytes",
                    message.Id, message.RetryCount, message.Payload.Length);

                // Send command to Application layer with raw Protobuf payload
                var command = new QueueTransactionCommand(message.Payload, message.Id, _queueNames.TransactionCreated);
                await mediator.Send(command, cancellationToken);

                // Mark as published (sets status to Published, records PublishedOn timestamp)
                message.MarkAsPublished();
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully processed outbox message - MessageId: {MessageId}",
                    message.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process outbox message - MessageId: {MessageId}, RetryCount: {RetryCount}, " +
                "Error: {ErrorMessage}",
                message.Id, message.RetryCount, ex.Message);

            // Record failure (increments retry_count, sets last_error, status = Failed)
            var errorMessage = $"{ex.GetType().Name}: {ex.Message}";
            if (errorMessage.Length > 500)
                errorMessage = errorMessage[..497] + "...";

            message.RecordFailure(errorMessage);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx,
                    "Failed to save error status for outbox message - MessageId: {MessageId}",
                    message.Id);
                // Message will remain in its current state and be retried on next poll
            }

            // Continue processing remaining messages (don't throw)
        }
    }
}
