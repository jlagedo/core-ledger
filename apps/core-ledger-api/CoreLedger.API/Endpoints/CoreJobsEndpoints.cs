using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.CoreJobs.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing CoreJob resources.
/// </summary>
public static class CoreJobsEndpoints
{
    public static IEndpointRouteBuilder MapCoreJobsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/corejobs")
            .WithTags("Core Jobs")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllCoreJobs");

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
        var query = new GetCoreJobsWithQueryQuery(
            limit == 0 ? 100 : limit,
            offset,
            sortBy,
            string.IsNullOrEmpty(sortDirection) ? "asc" : sortDirection,
            filter);

        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
