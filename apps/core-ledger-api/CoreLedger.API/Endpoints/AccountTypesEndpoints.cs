using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.AccountTypes.Commands;
using CoreLedger.Application.UseCases.AccountTypes.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing Account Type resources.
/// </summary>
public static class AccountTypesEndpoints
{
    public static IEndpointRouteBuilder MapAccountTypesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/accounttypes")
            .WithTags("Account Types")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllAccountTypes");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetAccountTypeById");

        group.MapPost("/", Create)
            .WithName("CreateAccountType");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateAccountType");

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteAccountType");

        return group;
    }

    private static async Task<IResult> GetAll(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllAccountTypesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAccountTypeByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateAccountTypeDto dto,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountTypeCommand(dto.Description);
        var result = await mediator.Send(command, cancellationToken);
        return Results.CreatedAtRoute("GetAccountTypeById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        int id,
        UpdateAccountTypeDto dto,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAccountTypeCommand(id, dto.Description);
        await mediator.Send(command, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAccountTypeCommand(id);
        await mediator.Send(command, cancellationToken);
        return Results.NoContent();
    }
}
