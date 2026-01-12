using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Indexadores.Commands;
using CoreLedger.Application.UseCases.Indexadores.Queries;
using CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;
using CoreLedger.Application.UseCases.HistoricosIndexadores.Queries;
using CoreLedger.Domain.Models;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing Indexador resources.
/// </summary>
public static class IndexadoresEndpoints
{
    private static readonly string LoggerName = typeof(IndexadoresEndpoints).Name;

    public static IEndpointRouteBuilder MapIndexadoresEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/indexadores")
            .WithTags("Indexadores")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllIndexadores");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetIndexadorById");

        group.MapGet("/{id:int}/historico", GetHistorico)
            .WithName("GetIndexadorHistorico");

        group.MapGet("/{id:int}/historico/exportar", ExportHistorico)
            .WithName("ExportIndexadorHistorico")
            .Produces(StatusCodes.Status200OK, contentType: "text/csv");

        group.MapPost("/{id:int}/historico/importar", ImportHistorico)
            .WithName("ImportIndexadorHistorico")
            .DisableAntiforgery();

        group.MapPost("/", Create)
            .WithName("CreateIndexador");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateIndexador");

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteIndexador");

        group.MapPost("/{id:int}/importar", Importar)
            .WithName("ImportarIndexador");

        return group;
    }

    private static async Task<IResult> GetAll(
        [AsParameters] IndexadorPaginationParameters pagination,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving indexadores - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, Tipo: {Tipo}, Ativo: {Ativo}, User: {UserId}",
            pagination.Limit, pagination.Offset, pagination.SortBy ?? "none", pagination.Filter ?? "none",
            pagination.Tipo?.ToString() ?? "none", pagination.Ativo?.ToString() ?? "none", userId);

        var query = new GetIndexadoresWithQueryQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            pagination.Filter,
            pagination.Tipo,
            pagination.Periodicidade,
            pagination.Fonte,
            pagination.Ativo,
            pagination.ImportacaoAutomatica);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Indexadores retrieved - Returned: {Count} of {Total} total indexadores",
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

        logger.LogInformation("Retrieving indexador {IndexadorId} for user {UserId}", id, userId);

        var query = new GetIndexadorByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Indexador retrieved - Codigo: {Codigo}, Nome: {Nome}, Tipo: {Tipo}",
            result.Codigo, result.Nome, result.TipoDescricao);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetHistorico(
        int id,
        [AsParameters] PaginationParameters pagination,
        DateOnly? dataInicio,
        DateOnly? dataFim,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving historical data for indexador {IndexadorId} - Limit: {Limit}, Offset: {Offset}, DataInicio: {DataInicio}, DataFim: {DataFim}, User: {UserId}",
            id, pagination.Limit, pagination.Offset, dataInicio?.ToString() ?? "none", dataFim?.ToString() ?? "none", userId);

        var parameters = new QueryParameters
        {
            Limit = pagination.Limit,
            Offset = pagination.Offset,
            SortBy = pagination.SortBy,
            SortDirection = pagination.SortDirection,
            Filter = pagination.Filter
        };

        var query = new GetHistoricoIndexadorQuery(id, parameters, dataInicio, dataFim);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Historical data retrieved - Returned: {Count} of {Total} total records for indexador {IndexadorId}",
            result.Items.Count, result.TotalCount, id);

        return Results.Ok(result);
    }

    private static async Task<IResult> ExportHistorico(
        int id,
        DateOnly? dataInicio,
        DateOnly? dataFim,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Exporting historical data for indexador {IndexadorId} - DataInicio: {DataInicio}, DataFim: {DataFim}, User: {UserId}",
            id, dataInicio?.ToString() ?? "none", dataFim?.ToString() ?? "none", userId);

        var query = new ExportHistoricoIndexadorQuery(id, dataInicio, dataFim);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Historical data exported for indexador {IndexadorId} - FileName: {FileName}",
            id, result.FileName);

        return Results.File(
            result.CsvContent,
            contentType: "text/csv; charset=utf-8",
            fileDownloadName: result.FileName);
    }

    private static async Task<IResult> ImportHistorico(
        int id,
        IFormFile file,
        bool sobrescrever,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "Arquivo CSV é obrigatório" });
        }

        // Validate file type
        if (!file.ContentType.Contains("csv") &&
            !file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { error = "Apenas arquivos CSV são aceitos" });
        }

        logger.LogInformation(
            "Importing historical data for indexador {IndexadorId} - FileName: {FileName}, Size: {Size}, Sobrescrever: {Sobrescrever}, User: {UserId}",
            id, file.FileName, file.Length, sobrescrever, userId);

        // Read file content
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var csvContent = memoryStream.ToArray();

        var command = new ImportHistoricoIndexadorCommand(id, csvContent, sobrescrever);
        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation(
            "Historical data import completed for indexador {IndexadorId} - Total: {Total}, Imported: {Imported}, Overwritten: {Overwritten}, Skipped: {Skipped}, Errors: {Errors}",
            id, result.TotalRows, result.ImportedRows, result.OverwrittenRows, result.SkippedRows, result.Errors.Count);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateIndexadorDto dto,
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
            "Creating indexador - Codigo: {Codigo}, Nome: {Nome}, Tipo: {Tipo}, CreatedBy: {UserId}",
            dto.Codigo, dto.Nome, dto.Tipo, userId);

        var command = new CreateIndexadorCommand(
            dto.Codigo,
            dto.Nome,
            dto.Tipo,
            dto.Fonte,
            dto.Periodicidade,
            dto.FatorAcumulado,
            dto.DataBase,
            dto.UrlFonte,
            dto.ImportacaoAutomatica,
            dto.Ativo);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Indexador created successfully - Id: {IndexadorId}, Codigo: {Codigo}",
            result.Id, result.Codigo);

        return Results.CreatedAtRoute("GetIndexadorById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        int id,
        UpdateIndexadorDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Updating indexador {IndexadorId} - Nome: {Nome}, UpdatedBy: {UserId}",
            id, dto.Nome, userId);

        var command = new UpdateIndexadorCommand(
            id,
            dto.Nome,
            dto.Fonte,
            dto.FatorAcumulado,
            dto.DataBase,
            dto.UrlFonte,
            dto.ImportacaoAutomatica,
            dto.Ativo);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Indexador updated successfully - Id: {IndexadorId}", id);

        return Results.Ok(result);
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

        logger.LogInformation("Deleting indexador {IndexadorId}, User: {UserId}", id, userId);

        var command = new DeleteIndexadorCommand(id);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Indexador deleted successfully - Id: {IndexadorId}", id);

        return Results.NoContent();
    }

    private static async Task<IResult> Importar(
        int id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();
        var correlationId = context.GetCorrelationId() ?? Guid.NewGuid().ToString();

        logger.LogInformation(
            "Triggering import for indexador {IndexadorId}, CorrelationId: {CorrelationId}, User: {UserId}",
            id, correlationId, userId);

        var command = new ImportarIndexadorCommand(id, correlationId);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation(
            "Import triggered successfully for indexador {IndexadorId}, CorrelationId: {CorrelationId}",
            id, correlationId);

        return Results.Accepted($"/api/indexadores/{id}/historico", new { id, correlationId });
    }
}
