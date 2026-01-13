using CoreLedger.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Serilog.Context;

namespace CoreLedger.API.Endpoints;

/// <summary>
/// Minimal API endpoints for Worker notifications.
/// Receives HTTP callbacks from the Worker service for transaction processing results.
/// </summary>
public static class WorkerNotificationsEndpoints
{
    private static readonly string LoggerName = typeof(WorkerNotificationsEndpoints).Name;

    public static IEndpointRouteBuilder MapWorkerNotificationsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/worker-notifications")
            .WithTags("Worker Notifications")
            .RequireAuthorization();

        group.MapPost("/transaction-processed", TransactionProcessed)
            .WithName("TransactionProcessed")
            .WithDescription("Receives transaction processing completion notifications from the Worker service")
            .Produces(StatusCodes.Status200OK);

        return group;
    }

    /// <summary>
    /// Receives notification from Worker that a transaction has been processed.
    /// Broadcasts to the user who created the transaction via SignalR.
    /// </summary>
    private static async Task<IResult> TransactionProcessed(
        TransactionProcessedNotification notification,
        IHubContext<NotificationHub> hubContext,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);

        using (LogContext.PushProperty("CorrelationId", notification.CorrelationId))
        {
            logger.LogInformation(
                "Transaction processing notification received - TransactionId: {TransactionId}, " +
                "Success: {Success}, FinalStatus: {Status}, UserId: {UserId}",
                notification.TransactionId,
                notification.Success,
                notification.FinalStatusId,
                notification.CreatedByUserId);

            // Send notification to the specific user who created the transaction
            var signalRMessage = new
            {
                message = notification.Success
                    ? $"Transaction {notification.TransactionId} processed successfully"
                    : $"Transaction {notification.TransactionId} failed: {notification.ErrorMessage ?? "Unknown error"}",
                type = notification.Success ? "success" : "error"
            };

            // Only send SignalR notification if we have a valid user ID
            if (!string.IsNullOrEmpty(notification.CreatedByUserId))
            {
                await hubContext.Clients
                    .User(notification.CreatedByUserId)
                    .SendAsync("ReceiveNotification", signalRMessage);

                logger.LogInformation(
                    "SignalR notification sent to user - TransactionId: {TransactionId}, UserId: {UserId}, Type: {Type}",
                    notification.TransactionId, notification.CreatedByUserId, signalRMessage.type);
            }
            else
            {
                logger.LogWarning(
                    "Cannot send SignalR notification - CreatedByUserId is null or empty for TransactionId: {TransactionId}",
                    notification.TransactionId);
            }

            return Results.Ok(new { Message = "Notification received and broadcast successfully" });
        }
    }
}

/// <summary>
/// Notification payload sent by Worker after processing a transaction.
/// </summary>
/// <param name="TransactionId">The ID of the processed transaction.</param>
/// <param name="Success">Whether the processing succeeded.</param>
/// <param name="FinalStatusId">The final status ID (2=Executed, 8=Failed).</param>
/// <param name="ErrorMessage">Error message if processing failed.</param>
/// <param name="ProcessedAt">UTC timestamp when processing completed.</param>
/// <param name="CorrelationId">Correlation ID for distributed tracing.</param>
/// <param name="CreatedByUserId">User ID of the person who created the transaction (may be null for batch operations).</param>
public record TransactionProcessedNotification(
    int TransactionId,
    bool Success,
    int FinalStatusId,
    string? ErrorMessage,
    DateTime ProcessedAt,
    string? CorrelationId,
    string? CreatedByUserId
);
