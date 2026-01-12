using CoreLedger.API.Extensions;
using CoreLedger.Application.DTOs.FundoClasse;
using CoreLedger.Application.UseCases.Cadastros.Classes.Commands;
using CoreLedger.Application.UseCases.Cadastros.Classes.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints.Cadastros;

/// <summary>
///     Minimal API endpoints for managing FundoClasse resources.
/// </summary>
public static class ClassesEndpoints
{
    private static readonly string LoggerName = typeof(ClassesEndpoints).Name;

    public static IEndpointRouteBuilder MapClassesEndpoints(this IEndpointRouteBuilder routes)
    {
        // Nested routes under fundos
        var fundoGroup = routes.MapGroup("/api/v1/fundos/{fundoId:guid}/classes")
            .WithTags("Classes")
            .RequireAuthorization();

        fundoGroup.MapGet("/", GetByFundo)
            .WithName("GetClassesByFundo");

        fundoGroup.MapPost("/", Create)
            .WithName("CreateClasse");

        // Direct routes for classe operations
        var classeGroup = routes.MapGroup("/api/v1/classes")
            .WithTags("Classes")
            .RequireAuthorization();

        classeGroup.MapGet("/{id:guid}", GetById)
            .WithName("GetClasseById");

        classeGroup.MapPut("/{id:guid}", Update)
            .WithName("UpdateClasse");

        classeGroup.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteClasse");

        return routes;
    }

    private static async Task<IResult> GetByFundo(
        Guid fundoId,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Retrieving classes for fundo {FundoId}, user {UserId}", fundoId, userId);

        var query = new GetClassesByFundoQuery(fundoId);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Classes retrieved - Count: {Count} for fundo {FundoId}", result.Count, fundoId);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Retrieving classe {ClasseId} for user {UserId}", id, userId);

        var query = new GetClasseByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        Guid fundoId,
        FundoClasseCreateDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Creating classe - FundoId: {FundoId}, Codigo: {Codigo}, CreatedBy: {UserId}",
            fundoId, dto.CodigoClasse, userId);

        var command = new CreateClasseCommand(
            fundoId,
            dto.CodigoClasse,
            dto.NomeClasse,
            dto.CnpjClasse,
            dto.TipoClasseFidc,
            dto.OrdemSubordinacao,
            dto.RentabilidadeAlvo,
            dto.ResponsabilidadeLimitada,
            dto.SegregacaoPatrimonial,
            dto.ValorMinimoAplicacao);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Classe created - Id: {ClasseId}", result.Id);

        return Results.CreatedAtRoute("GetClasseById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        Guid id,
        FundoClasseUpdateDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Updating classe {ClasseId} by user {UserId}", id, userId);

        var command = new UpdateClasseCommand(
            id,
            dto.NomeClasse,
            dto.CnpjClasse,
            dto.TipoClasseFidc,
            dto.OrdemSubordinacao,
            dto.RentabilidadeAlvo,
            dto.ResponsabilidadeLimitada,
            dto.SegregacaoPatrimonial,
            dto.ValorMinimoAplicacao);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Classe updated - Id: {ClasseId}", id);

        return Results.Ok(result);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Deleting classe {ClasseId} by user {UserId}", id, userId);

        var command = new DeleteClasseCommand(id);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Classe deleted - Id: {ClasseId}", id);

        return Results.NoContent();
    }
}
