using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.TransactionTypes.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing TransactionType resources.
/// </summary>
public static class TransactionTypesEndpoints
{
    public static IEndpointRouteBuilder MapTransactionTypesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/transactions/types")
            .WithTags("Transaction Types")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllTransactionTypes")
            .Produces<IReadOnlyList<TransactionTypeDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", GetById)
            .WithName("GetTransactionTypeById")
            .Produces<TransactionTypeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTransactionTypesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetTransactionTypeByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
