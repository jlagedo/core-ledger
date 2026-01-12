using MediatR;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

/// <summary>
/// Command to process a transaction by validating domain rules and updating status.
/// Executed by the Worker after receiving a transaction created event from RabbitMQ.
/// </summary>
/// <param name="TransactionId">The ID of the transaction to process.</param>
/// <param name="CorrelationId">Correlation ID for distributed tracing.</param>
public record ProcessTransactionCommand(
    int TransactionId,
    string CorrelationId
) : IRequest<ProcessTransactionResult>;

/// <summary>
/// Result of transaction processing with success/failure details.
/// </summary>
/// <param name="Success">Whether the transaction processing succeeded.</param>
/// <param name="TransactionId">The ID of the processed transaction.</param>
/// <param name="FinalStatusId">The final status ID (2=Executed, 8=Failed).</param>
/// <param name="CreatedByUserId">User ID of the person who created the transaction.</param>
/// <param name="ErrorMessage">Error message if processing failed.</param>
public record ProcessTransactionResult(
    bool Success,
    int TransactionId,
    int FinalStatusId,
    string CreatedByUserId,
    string? ErrorMessage = null
);
