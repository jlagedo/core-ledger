using CoreLedger.API.Extensions;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Cadastros.Vinculos.Commands;
using CoreLedger.Application.UseCases.Cadastros.Vinculos.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints.Cadastros;

/// <summary>
///     Minimal API endpoints for managing FundoVinculo resources.
/// </summary>
public static class VinculosEndpoints
{
    private static readonly string LoggerName = typeof(VinculosEndpoints).Name;

    public static IEndpointRouteBuilder MapVinculosEndpoints(this IEndpointRouteBuilder routes)
    {
        // Nested routes under fundos
        var fundoGroup = routes.MapGroup("/api/v1/fundos/{fundoId:guid}/vinculos")
            .WithTags("Vinculos")
            .RequireAuthorization();

        fundoGroup.MapGet("/", GetByFundo)
            .WithName("GetVinculosByFundo");

        fundoGroup.MapPost("/", Create)
            .WithName("CreateVinculo");

        // Direct routes for vinculo operations
        var vinculoGroup = routes.MapGroup("/api/v1/vinculos")
            .WithTags("Vinculos")
            .RequireAuthorization();

        vinculoGroup.MapPatch("/{id:long}/encerrar", Encerrar)
            .WithName("EncerrarVinculo");

        return routes;
    }

    private static async Task<IResult> GetByFundo(
        Guid fundoId,
        bool incluirEncerrados,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving vínculos for fundo {FundoId}, incluirEncerrados: {IncluirEncerrados}, user {UserId}",
            fundoId, incluirEncerrados, userId);

        var query = new GetVinculosByFundoQuery(fundoId, incluirEncerrados);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Vínculos retrieved - Count: {Count} for fundo {FundoId}", result.Count, fundoId);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        Guid fundoId,
        CreateFundoVinculoDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Creating vínculo - FundoId: {FundoId}, InstituicaoId: {InstituicaoId}, TipoVinculo: {TipoVinculo}, CreatedBy: {UserId}",
            fundoId, dto.InstituicaoId, dto.TipoVinculo, userId);

        var command = new CreateVinculoCommand(
            fundoId,
            dto.InstituicaoId,
            dto.TipoVinculo,
            dto.DataInicio,
            dto.Principal,
            dto.ContratoNumero,
            dto.Observacao);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Vínculo created - Id: {VinculoId}", result.Id);

        return Results.Created($"/api/v1/vinculos/{result.Id}", result);
    }

    private static async Task<IResult> Encerrar(
        long id,
        EncerrarVinculoDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Encerrando vínculo {VinculoId} by user {UserId}", id, userId);

        var command = new EncerrarVinculoCommand(id, dto.DataFim);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Vínculo encerrado - Id: {VinculoId}", id);

        return Results.NoContent();
    }
}
