using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.Accounts.Commands;
using CoreLedger.Application.UseCases.Accounts.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing Account resources.
/// </summary>
public static class AccountsEndpoints
{
    private static readonly string LoggerName = typeof(AccountsEndpoints).Name;
    public static IEndpointRouteBuilder MapAccountsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/accounts")
            .WithTags("Accounts")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllAccounts")
            .Produces<PagedResult<AccountDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", GetById)
            .WithName("GetAccountsById")
            .Produces<AccountDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/reports/by-type", GetAccountsByTypeReport)
            .WithName("GetAccountsByTypeReport")
            .Produces<IReadOnlyList<AccountsByTypeReportDto>>(StatusCodes.Status200OK);

        group.MapPost("/", Create)
            .WithName("CreateAccount")
            .Produces<AccountDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateAccount")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/deactivate", Deactivate)
            .WithName("DeactivateAccount")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

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

        logger.LogInformation(
            "Retrieving accounts - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, User: {UserId}",
            pagination.Limit, pagination.Offset, pagination.SortBy ?? "none", pagination.Filter ?? "none", userId);

        var query = new GetAccountsWithQueryQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            pagination.Filter);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Accounts retrieved - Returned: {Count} of {Total} total accounts",
            result.Items.Count, result.TotalCount);

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

        logger.LogInformation("Retrieving account {AccountId} for user {UserId}", id, userId);

        var query = new GetAccountByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Account retrieved - Code: {Code}, Name: {Name}", result.Code, result.Name);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetAccountsByTypeReport(
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Retrieving account type report for user {UserId}", userId);

        var query = new GetAccountsByTypeReportQuery();
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Account type report retrieved - {Count} account types", result.Count);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateAccountDto dto,
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

        logger.LogInformation(
            "Creating account - Code: {Code}, Name: {Name}, Type: {TypeId}, NormalBalance: {NormalBalance}, CreatedBy: {UserId}",
            dto.Code, dto.Name, dto.TypeId, dto.NormalBalance, userId);

        var command = new CreateAccountCommand(
            dto.Code,
            dto.Name,
            dto.TypeId,
            dto.Status,
            dto.NormalBalance,
            userId);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Account created successfully - Id: {AccountId}, Code: {Code}", result.Id, result.Code);

        return Results.CreatedAtRoute("GetAccountsById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        int id,
        UpdateAccountDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Updating account {AccountId} - Code: {Code}, Name: {Name}, Type: {TypeId}, NormalBalance: {NormalBalance}, UpdatedBy: {UserId}",
            id, dto.Code, dto.Name, dto.TypeId, dto.NormalBalance, userId);

        var command = new UpdateAccountCommand(
            id,
            dto.Code,
            dto.Name,
            dto.TypeId,
            dto.Status,
            dto.NormalBalance);

        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Account updated successfully - Id: {AccountId}", id);

        return Results.NoContent();
    }

    private static async Task<IResult> Deactivate(
        int id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Deactivating account {AccountId} - DeactivatedBy: {UserId}", id, userId);

        var command = new DeactivateAccountCommand(id);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Account deactivated successfully - Id: {AccountId}", id);

        return Results.NoContent();
    }
}
