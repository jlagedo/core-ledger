using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Extensions;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

public class CreateTransactionCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ILogger<CreateTransactionCommandHandler> logger)
    : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processando solicitação de criação de transação - ChaveIdempotência: {IdempotencyKey}, FundoId: {FundId}, " +
            "SubTipo: {SubTypeId}, Valor: {Amount}, IdCorrelação: {CorrelationId}",
            request.IdempotencyKey, request.FundId, request.TransactionSubTypeId,
            request.Amount, request.CorrelationId);

        try
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(ProcessTransactionAsync);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Falha na estratégia de execução para criação de transação - ChaveIdempotência: {IdempotencyKey}, FundoId: {FundId}",
                request.IdempotencyKey, request.FundId);
            throw;
        }

        async Task<TransactionDto> ProcessTransactionAsync()
        {
            // Begin explicit database transaction for atomicity
            await using IDbContextTransaction dbTransaction =
                await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // STEP 1: Check idempotency (fail fast before validation)
                var existingTransaction = await TryGetIdempotentTransactionAsync(
                    request.IdempotencyKey, cancellationToken);

                if (existingTransaction != null)
                {
                    await dbTransaction.CommitAsync(cancellationToken);
                    return existingTransaction;
                }

                // STEP 2: Validate foreign keys using extension method
                await context.Funds.ValidateEntityExistsAsync(
                    [request.FundId], "Fund", logger, cancellationToken);

                if (request.SecurityId.HasValue)
                    await context.Securities.ValidateEntityExistsAsync(
                        [request.SecurityId.Value], "Security", logger, cancellationToken);

                await context.TransactionSubTypes.ValidateEntityExistsAsync(
                    [request.TransactionSubTypeId], "TransactionSubType", logger, cancellationToken);

                // STEP 3: Create transaction entity
                var transaction = Transaction.Create(
                    request.FundId,
                    request.SecurityId,
                    request.TransactionSubTypeId,
                    request.TradeDate,
                    request.SettleDate,
                    request.Quantity,
                    request.Price,
                    request.Amount,
                    request.Currency,
                    1,
                    request.CreatedByUserId);

                context.Transactions.Add(transaction);

                // STEP 4: First SaveChanges to get database-generated Transaction.Id
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Entidade de transação persistida - IdTransação: {TransactionId}, ChaveIdempotência: {IdempotencyKey}",
                    transaction.Id, request.IdempotencyKey);

                // STEP 5: Reload transaction with navigation properties using extension
                var transactionWithNav = await context.Transactions
                    .WithNavigationProperties()
                    .FirstOrDefaultAsync(t => t.Id == transaction.Id, cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"Falha ao recarregar transação {transaction.Id} após persistência");

                // STEP 6: Create idempotency record with transaction ID
                var idempotencyRecord = TransactionIdempotency.Create(
                    request.IdempotencyKey,
                    transaction.Id);

                context.TransactionIdempotencies.Add(idempotencyRecord);

                // STEP 7: Create domain event using extension method
                var domainEvent = transactionWithNav.ToTransactionCreatedEvent(
                    request.CorrelationId, request.RequestId);

                // STEP 8: Serialize event to Protobuf using extension
                var eventPayload = domainEvent.SerializeToProtobuf();

                logger.LogDebug(
                    "TransactionCreatedEvent serializado - Tamanho: {PayloadSize} bytes, IdTransação: {TransactionId}",
                    eventPayload.Length, transaction.Id);

                // STEP 9: Create outbox message
                var outboxMessage = TransactionCreatedOutboxMessage.Create(
                    type: typeof(Events.TransactionCreatedEvent).FullName ?? nameof(Events.TransactionCreatedEvent),
                    payload: eventPayload,
                    occurredOn: domainEvent.OccurredOn);

                context.TransactionCreatedOutboxMessages.Add(outboxMessage);

                // STEP 10: Create audit log
                var transactionDto = mapper.Map<TransactionDto>(transactionWithNav);
                await CreateAuditLogAsync(transaction, transactionDto, request);

                // STEP 11: Second SaveChanges - persist idempotency, outbox, and audit atomically
                await context.SaveChangesAsync(cancellationToken);

                // STEP 12: Commit database transaction
                await dbTransaction.CommitAsync(cancellationToken);

                logger.LogInformation(
                    "Criação de transação concluída - IdTransação: {TransactionId}, ChaveIdempotência: {IdempotencyKey}, " +
                    "Valor: {Amount}, Status: {StatusId}, IdMensagemOutbox: {OutboxMessageId}",
                    transaction.Id, request.IdempotencyKey, transaction.Amount,
                    transaction.StatusId, outboxMessage.Id);

                return transactionDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Falha na criação de transação - ChaveIdempotência: {IdempotencyKey}, FundoId: {FundId}, " +
                    "Revertendo transação",
                    request.IdempotencyKey, request.FundId);

                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    /// <summary>
    /// Checks for existing idempotent transaction and returns it if found.
    /// </summary>
    private async Task<TransactionDto?> TryGetIdempotentTransactionAsync(
        Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var existingIdempotency = await context.TransactionIdempotencies
            .AsNoTracking()
            .FirstOrDefaultAsync(ti => ti.IdempotencyKey == idempotencyKey, cancellationToken);

        if (existingIdempotency?.TransactionId == null)
        {
            return null;
        }

        logger.LogInformation(
            "Solicitação idempotente detectada - ChaveIdempotência: {IdempotencyKey}, " +
            "Retornando IdTransação existente: {TransactionId}",
            idempotencyKey, existingIdempotency.TransactionId);

        // Load existing transaction with navigation properties using extension
        var existingTransaction = await context.Transactions
            .AsNoTracking()
            .WithNavigationProperties()
            .FirstOrDefaultAsync(t => t.Id == existingIdempotency.TransactionId, cancellationToken)
            ?? throw new DomainValidationException(
                $"Idempotency record exists but transaction {existingIdempotency.TransactionId} not found");

        return mapper.Map<TransactionDto>(existingTransaction);
    }

    /// <summary>
    /// Creates audit log entry for the transaction creation.
    /// </summary>
    private Task CreateAuditLogAsync(
        Transaction transaction,
        TransactionDto transactionDto,
        CreateTransactionCommand request)
    {
        var transactionDataJson = JsonSerializer.Serialize(transactionDto);
        var dataAfter = JsonDocument.Parse(transactionDataJson);

        var auditLog = AuditLog.Create(
            entityName: "Transaction",
            entityId: transaction.Id.ToString(),
            eventType: "Created",
            performedByUserId: request.CreatedByUserId,
            dataAfter: dataAfter,
            correlationId: request.CorrelationId,
            requestId: request.RequestId,
            source: "API");

        context.AuditLogs.Add(auditLog);
        return Task.CompletedTask;
    }
}
