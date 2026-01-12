using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.Transactions.Commands;
using CoreLedger.Application.UseCases.Transactions.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing Transaction resources.
/// </summary>
public static class TransactionsEndpoints
{
    private static readonly string LoggerName = nameof(TransactionsEndpoints);
    public static IEndpointRouteBuilder MapTransactionsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/transactions")
            .WithTags("Transactions")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllTransactions");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetTransactionById");

        group.MapPost("/", Create)
            .WithName("CreateTransaction");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateTransaction");

        return group;
    }

    private static async Task<IResult> GetAll(
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();
        var correlationId = context.GetCorrelationId();

        logger.LogInformation(
            "Retrieving transactions - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, CorrelationId: {CorrelationId}, User: {UserId}",
            pagination.Limit, pagination.Offset, pagination.SortBy ?? "none", pagination.Filter ?? "none", correlationId, userId);

        var query = new GetTransactionsWithQueryQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            pagination.Filter);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Transactions retrieved - Returned: {Count} of {Total} total transactions, CorrelationId: {CorrelationId}",
            result.Items.Count, result.TotalCount, correlationId);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        int id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();
        var correlationId = context.GetCorrelationId();

        logger.LogInformation("Retrieving transaction {TransactionId} for user {UserId}, CorrelationId: {CorrelationId}",
            id, userId, correlationId);

        var query = new GetTransactionByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Transaction retrieved - Amount: {Amount}, Fund: {FundId}, Status: {StatusId}, CorrelationId: {CorrelationId}",
            result.Amount, result.FundId, result.StatusId, correlationId);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateTransactionDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogError("Authentication failed: 'sub' claim missing from token for endpoint {Endpoint}",
                context.Request.Path);
            return Results.Unauthorized();
        }

        // Extract or generate idempotency key
        var idempotencyKey = dto.IdempotencyKey ?? Guid.CreateVersion7();

        if (!dto.IdempotencyKey.HasValue)
        {
            logger.LogInformation(
                "No Idempotency-Key provided, auto-generated UUID v7: {IdempotencyKey}",
                idempotencyKey);
        }
        else
        {
            logger.LogInformation(
                "Using client-provided Idempotency-Key: {IdempotencyKey}",
                idempotencyKey);
        }

        // Extract correlation ID and request ID from HttpContext for audit logging
        var correlationId = context.GetCorrelationId();
        var requestId = context.TraceIdentifier;

        logger.LogInformation(
            "Creating transaction - Fund: {FundId}, SubType: {SubTypeId}, Amount: {Amount}, " +
            "Quantity: {Quantity}, Price: {Price}, Currency: {Currency}, IdempotencyKey: {IdempotencyKey}, CreatedBy: {UserId}",
            dto.FundId, dto.TransactionSubTypeId, dto.Amount,
            dto.Quantity, dto.Price, dto.Currency, idempotencyKey, userId);

        var command = new CreateTransactionCommand(
            dto.FundId,
            dto.SecurityId,
            dto.TransactionSubTypeId,
            dto.TradeDate,
            dto.SettleDate,
            dto.Quantity,
            dto.Price,
            dto.Amount,
            dto.Currency,
            userId,
            idempotencyKey,
            correlationId,
            requestId);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation(
            "Transaction created successfully - Id: {TransactionId}, Amount: {Amount}, IdempotencyKey: {IdempotencyKey}",
            result.Id, result.Amount, idempotencyKey);

        return Results.CreatedAtRoute("GetTransactionById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        int id,
        UpdateTransactionDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Updating transaction {TransactionId} - Amount: {Amount}, Quantity: {Quantity}, " +
            "Price: {Price}, Currency: {Currency}, Status: {StatusId}, UpdatedBy: {UserId}",
            id, dto.Amount, dto.Quantity, dto.Price, dto.Currency, dto.StatusId, userId);

        var command = new UpdateTransactionCommand(
            id,
            dto.FundId,
            dto.SecurityId,
            dto.TransactionSubTypeId,
            dto.TradeDate,
            dto.SettleDate,
            dto.Quantity,
            dto.Price,
            dto.Amount,
            dto.Currency,
            dto.StatusId);

        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Transaction updated successfully - Id: {TransactionId}", id);

        return Results.NoContent();
    }
}
