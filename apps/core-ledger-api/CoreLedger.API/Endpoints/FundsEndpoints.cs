using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.Funds.Commands;
using CoreLedger.Application.UseCases.Funds.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing Fund resources.
/// </summary>
public static class FundsEndpoints
{
    private static readonly string LoggerName = typeof(FundsEndpoints).Name;
    public static IEndpointRouteBuilder MapFundsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/funds")
            .WithTags("Funds")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllFunds");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetFundById");

        group.MapGet("/autocomplete", Autocomplete)
            .WithName("AutocompleteFunds");

        group.MapPost("/", Create)
            .WithName("CreateFund");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateFund");

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
            "Retrieving funds - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, User: {UserId}",
            pagination.Limit, pagination.Offset, pagination.SortBy ?? "none", pagination.Filter ?? "none", userId);

        var query = new GetFundsWithQueryQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            pagination.Filter);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Funds retrieved - Returned: {Count} of {Total} total funds",
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

        logger.LogInformation("Retrieving fund {FundId} for user {UserId}", id, userId);

        var query = new GetFundByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Fund retrieved - Code: {Code}, Name: {Name}, Currency: {Currency}",
            result.Code, result.Name, result.BaseCurrency);

        return Results.Ok(result);
    }

    private static async Task<IResult> Autocomplete(
        [FromQuery] string? q,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Autocomplete search for funds - Query: {Query}, User: {UserId}",
            q ?? "<empty>", userId);

        var query = new AutocompleteFundsQuery(q);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Autocomplete returned {Count} funds for query: {Query}",
            result.Count, q ?? "<empty>");

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateFundDto dto,
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
            "Creating fund - Code: {Code}, Name: {Name}, Currency: {Currency}, CreatedBy: {UserId}",
            dto.Code, dto.Name, dto.BaseCurrency, userId);

        var command = new CreateFundCommand(
            dto.Code,
            dto.Name,
            dto.BaseCurrency,
            dto.InceptionDate,
            dto.ValuationFrequency,
            userId);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Fund created successfully - Id: {FundId}, Code: {Code}", result.Id, result.Code);

        return Results.CreatedAtRoute("GetFundById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        int id,
        UpdateFundDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Updating fund {FundId} - Code: {Code}, Name: {Name}, Currency: {Currency}, UpdatedBy: {UserId}",
            id, dto.Code, dto.Name, dto.BaseCurrency, userId);

        var command = new UpdateFundCommand(
            id,
            dto.Code,
            dto.Name,
            dto.BaseCurrency,
            dto.InceptionDate,
            dto.ValuationFrequency);

        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Fund updated successfully - Id: {FundId}", id);

        return Results.NoContent();
    }
}
