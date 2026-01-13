using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.TransactionStatuses.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing TransactionStatus resources.
/// </summary>
public static class TransactionStatusesEndpoints
{
    public static IEndpointRouteBuilder MapTransactionStatusesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/transactions/status")
            .WithTags("Transaction Statuses")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllTransactionStatuses")
            .Produces<IReadOnlyList<TransactionStatusDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", GetById)
            .WithName("GetTransactionStatusById")
            .Produces<TransactionStatusDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTransactionStatusesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetTransactionStatusByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
