using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.AuditLogs.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for retrieving audit log entries.
/// </summary>
public static class AuditLogsEndpoints
{
    public static IEndpointRouteBuilder MapAuditLogsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auditlogs")
            .WithTags("Audit Logs")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllAuditLogs");

        return group;
    }

    private static async Task<IResult> GetAll(
        int limit,
        int offset,
        string? sortBy,
        string sortDirection,
        string? filter,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAuditLogsWithQueryQuery(
            limit == 0 ? 100 : limit,
            offset,
            sortBy,
            string.IsNullOrEmpty(sortDirection) ? "desc" : sortDirection,
            filter);

        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
