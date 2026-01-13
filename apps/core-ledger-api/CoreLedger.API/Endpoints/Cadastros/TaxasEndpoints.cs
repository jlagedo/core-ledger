using CoreLedger.API.Extensions;
using CoreLedger.Application.DTOs.FundoTaxa;
using CoreLedger.Application.UseCases.Cadastros.Taxas.Commands;
using CoreLedger.Application.UseCases.Cadastros.Taxas.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints.Cadastros;

/// <summary>
///     Minimal API endpoints for managing FundoTaxa resources.
/// </summary>
public static class TaxasEndpoints
{
    private static readonly string LoggerName = typeof(TaxasEndpoints).Name;

    public static IEndpointRouteBuilder MapTaxasEndpoints(this IEndpointRouteBuilder routes)
    {
        // Nested routes under fundos
        var fundoGroup = routes.MapGroup("/api/v1/fundos/{fundoId:guid}/taxas")
            .WithTags("Taxas")
            .RequireAuthorization();

        fundoGroup.MapGet("/", GetByFundo)
            .WithName("GetTaxasByFundo")
            .Produces<IReadOnlyList<FundoTaxaListDto>>(StatusCodes.Status200OK);

        fundoGroup.MapPost("/", Create)
            .WithName("CreateTaxa")
            .Produces<FundoTaxaResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // Direct routes for taxa operations
        var taxaGroup = routes.MapGroup("/api/v1/taxas")
            .WithTags("Taxas")
            .RequireAuthorization();

        taxaGroup.MapPut("/{id:long}", Update)
            .WithName("UpdateTaxa")
            .Produces<FundoTaxaResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        taxaGroup.MapDelete("/{id:long}", Delete)
            .WithName("DeleteTaxa")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> GetByFundo(
        Guid fundoId,
        bool incluirInativas,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving taxas for fundo {FundoId}, incluirInativas: {IncluirInativas}, user {UserId}",
            fundoId, incluirInativas, userId);

        var query = new GetTaxasByFundoQuery(fundoId, incluirInativas);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Taxas retrieved - Count: {Count} for fundo {FundoId}", result.Count, fundoId);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        Guid fundoId,
        FundoTaxaCreateDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Creating taxa - FundoId: {FundoId}, TipoTaxa: {TipoTaxa}, CreatedBy: {UserId}",
            fundoId, dto.TipoTaxa, userId);

        var command = new CreateTaxaCommand(
            fundoId,
            dto.TipoTaxa,
            dto.Percentual,
            dto.BaseCalculo,
            dto.PeriodicidadeProvisao,
            dto.PeriodicidadePagamento,
            dto.DataInicioVigencia,
            dto.ClasseId,
            dto.DiaPagamento,
            dto.ValorMinimo,
            dto.ValorMaximo,
            dto.ParametrosPerformance);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Taxa created - Id: {TaxaId}", result.Id);

        return Results.Created($"/api/v1/taxas/{result.Id}", result);
    }

    private static async Task<IResult> Update(
        long id,
        FundoTaxaUpdateDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Updating taxa {TaxaId} by user {UserId}", id, userId);

        var command = new UpdateTaxaCommand(
            id,
            dto.Percentual,
            dto.BaseCalculo,
            dto.PeriodicidadeProvisao,
            dto.PeriodicidadePagamento,
            dto.DiaPagamento,
            dto.ValorMinimo,
            dto.ValorMaximo);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Taxa updated - Id: {TaxaId}", id);

        return Results.Ok(result);
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

        logger.LogInformation("Deleting (deactivating) taxa {TaxaId} by user {UserId}", id, userId);

        var command = new DeleteTaxaCommand(id);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Taxa deactivated - Id: {TaxaId}", id);

        return Results.NoContent();
    }
}
