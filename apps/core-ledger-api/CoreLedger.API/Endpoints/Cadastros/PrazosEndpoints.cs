using CoreLedger.API.Extensions;
using CoreLedger.Application.DTOs.FundoPrazo;
using CoreLedger.Application.UseCases.Cadastros.Prazos.Commands;
using CoreLedger.Application.UseCases.Cadastros.Prazos.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints.Cadastros;

/// <summary>
///     Minimal API endpoints for managing FundoPrazo resources.
/// </summary>
public static class PrazosEndpoints
{
    private static readonly string LoggerName = typeof(PrazosEndpoints).Name;

    public static IEndpointRouteBuilder MapPrazosEndpoints(this IEndpointRouteBuilder routes)
    {
        // Nested routes under fundos
        var fundoGroup = routes.MapGroup("/api/v1/fundos/{fundoId:guid}/prazos")
            .WithTags("Prazos")
            .RequireAuthorization();

        fundoGroup.MapGet("/", GetByFundo)
            .WithName("GetPrazosByFundo");

        fundoGroup.MapPost("/", Create)
            .WithName("CreatePrazo");

        // Direct routes for prazo operations
        var prazoGroup = routes.MapGroup("/api/v1/prazos")
            .WithTags("Prazos")
            .RequireAuthorization();

        prazoGroup.MapPut("/{id:long}", Update)
            .WithName("UpdatePrazo");

        return routes;
    }

    private static async Task<IResult> GetByFundo(
        Guid fundoId,
        bool incluirInativos,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving prazos for fundo {FundoId}, incluirInativos: {IncluirInativos}, user {UserId}",
            fundoId, incluirInativos, userId);

        var query = new GetPrazosByFundoQuery(fundoId, incluirInativos);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Prazos retrieved - Count: {Count} for fundo {FundoId}", result.Count, fundoId);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        Guid fundoId,
        FundoPrazoCreateDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Creating prazo - FundoId: {FundoId}, TipoPrazo: {TipoPrazo}, CreatedBy: {UserId}",
            fundoId, dto.TipoPrazo, userId);

        var command = new CreatePrazoCommand(
            fundoId,
            dto.TipoPrazo,
            dto.DiasCotizacao,
            dto.DiasLiquidacao,
            dto.HorarioLimite,
            dto.DiasUteis,
            dto.ClasseId,
            dto.DiasCarencia,
            dto.CalendarioId,
            dto.PermiteParcial,
            dto.PercentualMinimo,
            dto.ValorMinimo);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Prazo created - Id: {PrazoId}", result.Id);

        return Results.Created($"/api/v1/prazos/{result.Id}", result);
    }

    private static async Task<IResult> Update(
        long id,
        FundoPrazoUpdateDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Updating prazo {PrazoId} by user {UserId}", id, userId);

        var command = new UpdatePrazoCommand(
            id,
            dto.DiasCotizacao,
            dto.DiasLiquidacao,
            dto.HorarioLimite,
            dto.DiasUteis,
            dto.DiasCarencia,
            dto.CalendarioId,
            dto.PermiteParcial,
            dto.PercentualMinimo,
            dto.ValorMinimo);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Prazo updated - Id: {PrazoId}", id);

        return Results.Ok(result);
    }
}
