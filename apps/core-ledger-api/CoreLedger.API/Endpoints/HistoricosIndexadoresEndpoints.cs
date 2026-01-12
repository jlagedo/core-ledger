using CoreLedger.API.Extensions;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing HistoricoIndexador resources.
/// </summary>
public static class HistoricosIndexadoresEndpoints
{
    private static readonly string LoggerName = typeof(HistoricosIndexadoresEndpoints).Name;

    public static IEndpointRouteBuilder MapHistoricosIndexadoresEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/historicos-indexadores")
            .WithTags("HistoricosIndexadores")
            .RequireAuthorization();

        group.MapPost("/", Create)
            .WithName("CreateHistoricoIndexador");

        group.MapDelete("/{id:long}", Delete)
            .WithName("DeleteHistoricoIndexador");

        return group;
    }

    private static async Task<IResult> Create(
        CreateHistoricoIndexadorDto dto,
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
            "Creating historical record - IndexadorId: {IndexadorId}, DataReferencia: {DataReferencia:yyyy-MM-dd}, Valor: {Valor}, CreatedBy: {UserId}",
            dto.IndexadorId, dto.DataReferencia, dto.Valor, userId);

        var command = new CreateHistoricoIndexadorCommand(
            dto.IndexadorId,
            dto.DataReferencia,
            dto.Valor,
            dto.FatorDiario,
            dto.VariacaoPercentual,
            dto.Fonte,
            dto.ImportacaoId);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation(
            "Historical record created successfully - Id: {Id}, IndexadorId: {IndexadorId}, DataReferencia: {DataReferencia:yyyy-MM-dd}",
            result.Id, result.IndexadorId, result.DataReferencia);

        return Results.Created($"/api/historicos-indexadores/{result.Id}", result);
    }

    private static async Task<IResult> Delete(
        long id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Deleting historical record {HistoricoId}, User: {UserId}", id, userId);

        var command = new DeleteHistoricoIndexadorCommand(id);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Historical record deleted successfully - Id: {HistoricoId}", id);

        return Results.NoContent();
    }
}
