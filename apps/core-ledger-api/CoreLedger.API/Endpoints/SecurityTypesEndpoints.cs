using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Securities.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for retrieving SecurityType enum values.
/// </summary>
public static class SecurityTypesEndpoints
{
    public static IEndpointRouteBuilder MapSecurityTypesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/securitytypes")
            .WithTags("Security Types")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllSecurityTypes");

        return group;
    }

    private static async Task<IResult> GetAll(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllSecurityTypesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
