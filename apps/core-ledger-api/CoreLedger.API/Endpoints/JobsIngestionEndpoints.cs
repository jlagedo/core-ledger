using CoreLedger.API.Extensions;
using CoreLedger.Application.Constants;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for job ingestion operations.
/// </summary>
public static class JobsIngestionEndpoints
{
    private static readonly string LoggerName = typeof(JobsIngestionEndpoints).Name;
    public static IEndpointRouteBuilder MapJobsIngestionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/jobs-ingestion")
            .WithTags("Jobs Ingestion")
            .RequireAuthorization();

        group.MapPost("/import-b3-instruction-file", ImportB3InstructionFile)
            .WithName("ImportB3InstructionFile")
            .Produces<ImportB3InstructionFileResponse>(StatusCodes.Status202Accepted);

        group.MapPost("/test-connection", TestConnection)
            .WithName("TestConnection")
            .Produces<TestConnectionResponse>(StatusCodes.Status202Accepted);

        return group;
    }

    private static async Task<IResult> ImportB3InstructionFile(
        IApplicationDbContext context,
        IMessagePublisher messagePublisher,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);

        // Extract user and correlation ID from HttpContext
        var userId = httpContext.GetUserId() ?? "anonymous";
        var correlationId = httpContext.GetCorrelationId();

        // Auto-generate reference ID in format: CJB3-YYYYMMDDHHMMSS-XXXXXX (with unique suffix to prevent collisions)
        var now = DateTime.UtcNow;
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..6]; // 6-character unique suffix
        var referenceId = $"CJB3-{now:yyyyMMddHHmmss}-{uniqueSuffix}";

        // Auto-generate job description with current datetime
        var jobDescription = $"B3 import initialization started at {now:yyyy-MM-dd HH:mm:ss} UTC";

        logger.LogInformation(
            "Starting B3 instruction file import for ReferenceId: {ReferenceId}, CorrelationId: {CorrelationId}, InitiatedBy: {UserId}",
            referenceId, correlationId, userId);

        var coreJob = CoreJob.Create(
            referenceId,
            jobDescription);

        context.CoreJobs.Add(coreJob);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "CoreJob created with Id: {CoreJobId}, ReferenceId: {ReferenceId}, CorrelationId: {CorrelationId}, CreatedBy: {UserId}",
            coreJob.Id, coreJob.ReferenceId, correlationId, userId);

        var message = new CoreJobB3ImportMessage(
            coreJob.Id,
            coreJob.ReferenceId,
            "CoreJobB3Import",
            correlationId);

        await messagePublisher.PublishAsync(
            QueueNames.B3Import,
            message,
            correlationId,
            cancellationToken);

        logger.LogInformation(
            "Message published to {QueueName} for CoreJobId: {CoreJobId}, CorrelationId: {CorrelationId}, Publisher: {UserId}",
            QueueNames.B3Import, coreJob.Id, correlationId, userId);

        var response = new ImportB3InstructionFileResponse(
            coreJob.Id,
            coreJob.ReferenceId,
            "Accepted",
            "B3 instruction file import job has been queued successfully");

        return Results.Accepted(value: response);
    }

    private static async Task<IResult> TestConnection(
        TestConnectionRequest request,
        IApplicationDbContext context,
        IMessagePublisher messagePublisher,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);

        // Extract user and correlation ID from HttpContext
        var userId = httpContext.GetUserId() ?? "anonymous";
        var correlationId = httpContext.GetCorrelationId();

        logger.LogInformation(
            "Testing API -> Queue -> Worker connection for ReferenceId: {ReferenceId}, CorrelationId: {CorrelationId}, InitiatedBy: {UserId}",
            request.ReferenceId, correlationId, userId);

        var coreJob = CoreJob.Create(
            request.ReferenceId,
            request.JobDescription ?? "Test connection job");

        context.CoreJobs.Add(coreJob);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Test CoreJob created with Id: {CoreJobId}, Status: {Status}, CorrelationId: {CorrelationId}, CreatedBy: {UserId}",
            coreJob.Id, coreJob.Status, correlationId, userId);

        var message = new TestConnectionMessage(
            coreJob.Id,
            coreJob.ReferenceId,
            "TestConnection",
            correlationId);

        await messagePublisher.PublishAsync(
            QueueNames.TestConnection,
            message,
            correlationId,
            cancellationToken);

        logger.LogInformation(
            "Test message published to {QueueName} for CoreJobId: {CoreJobId}, CorrelationId: {CorrelationId}, Publisher: {UserId}",
            QueueNames.TestConnection, coreJob.Id, correlationId, userId);

        var response = new TestConnectionResponse(
            coreJob.Id,
            coreJob.ReferenceId,
            coreJob.Status.ToString(),
            "Test connection job has been queued successfully. Check worker logs to verify message processing.",
            correlationId);

        return Results.Accepted(value: response);
    }
}
