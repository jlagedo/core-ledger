using CoreLedger.API.Extensions;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Cadastros.ClassificacoesAnbima.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints.Cadastros;

/// <summary>
///     Minimal API endpoints for ANBIMA classifications lookup.
/// </summary>
public static class ClassificacoesAnbimaEndpoints
{
    private static readonly string LoggerName = typeof(ClassificacoesAnbimaEndpoints).Name;

    public static IEndpointRouteBuilder MapClassificacoesAnbimaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/parametros/classificacoes-anbima")
            .WithTags("ANBIMA Classifications")
            .RequireAuthorization();

        group.MapGet("/", Listar)
            .WithName("ListarClassificacoesAnbima")
            .WithDescription("Get all ANBIMA classifications with optional filters")
            .Produces<ListarClassificacoesAnbimaResponse>(StatusCodes.Status200OK);

        group.MapGet("/{codigo}", ObterPorCodigo)
            .WithName("ObterClassificacaoAnbimaPorCodigo")
            .WithDescription("Get a specific ANBIMA classification by its codigo")
            .Produces<ClassificacaoAnbimaDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/niveis", ListarNiveis)
            .WithName("ListarNiveisAnbima")
            .WithDescription("Get hierarchical levels (Nivel1, Nivel2) for ANBIMA classifications")
            .Produces<NiveisClassificacaoAnbimaResponse>(StatusCodes.Status200OK);

        group.MapGet("/verificar", VerificarCompatibilidade)
            .WithName("VerificarCompatibilidadeAnbima")
            .WithDescription("Verify compatibility between ANBIMA classification and CVM classification")
            .Produces<VerificarCompatibilidadeResponse>(StatusCodes.Status200OK);

        return routes;
    }

    /// <summary>
    ///     List ANBIMA classifications with optional filters.
    /// </summary>
    private static async Task<IResult> Listar(
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        string? classificacao_cvm = null,
        string? nivel1 = null,
        bool ativo = true,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Listing ANBIMA classifications - ClassificacaoCvm: {ClassificacaoCvm}, Nivel1: {Nivel1}, Ativo: {Ativo}, User: {UserId}",
            classificacao_cvm ?? "none", nivel1 ?? "none", ativo, userId);

        var query = new ListarClassificacoesAnbimaQuery(
            classificacao_cvm,
            nivel1,
            ativo);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "ANBIMA classifications retrieved - Count: {Count}",
            result.Total);

        return Results.Ok(result);
    }

    /// <summary>
    ///     Get ANBIMA classification by codigo.
    /// </summary>
    private static async Task<IResult> ObterPorCodigo(
        string codigo,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Getting ANBIMA classification by codigo - Codigo: {Codigo}, User: {UserId}",
            codigo, userId);

        var query = new ObterClassificacaoAnbimaPorCodigoQuery(codigo);

        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
        {
            logger.LogWarning("ANBIMA classification not found - Codigo: {Codigo}", codigo);
            return Results.NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Classificação ANBIMA não encontrada",
                status = 404,
                detail = $"Código '{codigo}' não encontrado"
            });
        }

        logger.LogInformation("ANBIMA classification found - Codigo: {Codigo}", codigo);

        return Results.Ok(result);
    }

    /// <summary>
    ///     Get hierarchical levels for ANBIMA classifications.
    /// </summary>
    private static async Task<IResult> ListarNiveis(
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        string? classificacao_cvm = null,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Getting hierarchical levels for ANBIMA classifications - ClassificacaoCvm: {ClassificacaoCvm}, User: {UserId}",
            classificacao_cvm ?? "none", userId);

        var query = new ListarNiveisAnbimaQuery(classificacao_cvm);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "ANBIMA hierarchical levels retrieved - Nivel1 count: {Nivel1Count}",
            result.Nivel1.Count);

        return Results.Ok(result);
    }

    /// <summary>
    ///     Verify compatibility between ANBIMA classification and CVM classification.
    /// </summary>
    private static async Task<IResult> VerificarCompatibilidade(
        string codigo_anbima,
        string classificacao_cvm,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Verifying compatibility - CodigoAnbima: {CodigoAnbima}, ClassificacaoCvm: {ClassificacaoCvm}, User: {UserId}",
            codigo_anbima, classificacao_cvm, userId);

        var query = new VerificarCompatibilidadeAnbimaQuery(codigo_anbima, classificacao_cvm);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Compatibility verification completed - Compativel: {Compativel}",
            result.Compativel);

        return Results.Ok(result);
    }
}
