using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Calendario.Commands;
using CoreLedger.Application.UseCases.Calendario.Queries;
using CoreLedger.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing Calendario resources (business days and holidays).
/// </summary>
public static class CalendarioEndpoints
{
    private static readonly string LoggerName = typeof(CalendarioEndpoints).Name;

    public static IEndpointRouteBuilder MapCalendarioEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/calendario")
            .WithTags("Calendário")
            .RequireAuthorization();

        // CRUD operations
        group.MapGet("/", GetAll)
            .WithName("GetAllCalendarios");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetCalendarioById");

        group.MapPost("/", Create)
            .WithName("CreateCalendario");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateCalendario");

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteCalendario");

        // Business operations
        group.MapGet("/dia-util/{data}", CheckDiaUtil)
            .WithName("CheckDiaUtil");

        group.MapGet("/proximo-dia-util/{data}", GetProximoDiaUtil)
            .WithName("GetProximoDiaUtil");

        group.MapGet("/calcular-d-mais/{data}/{dias:int}", CalcularDMais)
            .WithName("CalcularDMais");

        group.MapPost("/importar/{ano:int}", ImportarCalendario)
            .WithName("ImportarCalendario");

        group.MapGet("/health", CheckHealth)
            .WithName("CheckCalendarioHealth");

        return group;
    }

    private static async Task<IResult> GetAll(
        [AsParameters] CalendarioPaginationParameters pagination,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving calendarios - Limit: {Limit}, Offset: {Offset}, Search: {Search}, Praca: {Praca}, TipoDia: {TipoDia}, DiaUtil: {DiaUtil}, DataInicio: {DataInicio}, DataFim: {DataFim}, User: {UserId}",
            pagination.Limit, pagination.Offset, pagination.Search ?? "none",
            pagination.Praca?.ToString() ?? "none", pagination.TipoDia?.ToString() ?? "none",
            pagination.DiaUtil?.ToString() ?? "none", pagination.DataInicio ?? "none",
            pagination.DataFim ?? "none", userId);

        var query = new GetCalendariosWithQueryQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            pagination.Search,
            pagination.Praca,
            pagination.TipoDia,
            pagination.DiaUtil,
            pagination.DataInicio,
            pagination.DataFim);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Calendarios retrieved - Returned: {Count} of {Total} total",
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

        logger.LogInformation("User {UserId} retrieving Calendario with ID {Id}", userId, id);

        var query = new GetCalendarioByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateCalendarioDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "User {UserId} creating Calendario for {Data}, praca {Praca}",
            userId, dto.Data, dto.Praca);

        var command = new CreateCalendarioCommand(
            dto.Data,
            dto.TipoDia,
            dto.Praca,
            dto.Descricao,
            userId!);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation(
            "Calendario created with ID {Id} for {Data}",
            result.Id, result.Data);

        return Results.CreatedAtRoute("GetCalendarioById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        int id,
        UpdateCalendarioDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("User {UserId} updating Calendario with ID {Id}", userId, id);

        var command = new UpdateCalendarioCommand(id, dto.TipoDia, dto.Descricao);
        await mediator.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> Delete(
        int id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("User {UserId} deleting Calendario with ID {Id}", userId, id);

        var command = new DeleteCalendarioCommand(id);
        await mediator.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> CheckDiaUtil(
        string data,
        [FromQuery] Praca? praca,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        if (!DateOnly.TryParse(data, out var dateValue))
        {
            return Results.BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });
        }

        // Default to Nacional if not specified
        var pracaValue = praca ?? Praca.Nacional;

        logger.LogInformation(
            "User {UserId} checking if {Data} is business day for praca {Praca}",
            userId, dateValue, pracaValue);

        var query = new CheckDiaUtilQuery(dateValue, pracaValue);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetProximoDiaUtil(
        string data,
        [FromQuery] Praca? praca,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        if (!DateOnly.TryParse(data, out var dateValue))
        {
            return Results.BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });
        }

        // Default to Nacional if not specified
        var pracaValue = praca ?? Praca.Nacional;

        logger.LogInformation(
            "User {UserId} getting next business day after {Data} for praca {Praca}",
            userId, dateValue, pracaValue);

        var query = new GetProximoDiaUtilQuery(dateValue, pracaValue);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(new { data = result });
    }

    private static async Task<IResult> CalcularDMais(
        string data,
        int dias,
        [FromQuery] Praca? praca,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        if (!DateOnly.TryParse(data, out var dateValue))
        {
            return Results.BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });
        }

        // Default to Nacional if not specified
        var pracaValue = praca ?? Praca.Nacional;

        logger.LogInformation(
            "User {UserId} calculating D+{Dias} from {Data} for praca {Praca}",
            userId, dias, dateValue, pracaValue);

        var query = new CalcularDMaisQuery(dateValue, dias, pracaValue);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> ImportarCalendario(
        int ano,
        [FromQuery] Praca? praca,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        // Default to Nacional if not specified
        var pracaValue = praca ?? Praca.Nacional;

        logger.LogWarning(
            "User {UserId} calling STUB ImportarCalendario for year {Ano}, praca {Praca}",
            userId, ano, pracaValue);

        var command = new ImportarCalendarioCommand(ano, pracaValue, userId!);
        var result = await mediator.Send(command, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CheckHealth(
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogWarning("User {UserId} calling STUB CheckCalendarioHealth", userId);

        var query = new CheckCalendarioHealthQuery();
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }
}
