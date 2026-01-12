using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Cadastros.Instituicoes.Commands;
using CoreLedger.Application.UseCases.Cadastros.Instituicoes.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints.Cadastros;

/// <summary>
///     Minimal API endpoints for managing Instituicao resources.
/// </summary>
public static class InstituicoesEndpoints
{
    private static readonly string LoggerName = typeof(InstituicoesEndpoints).Name;

    public static IEndpointRouteBuilder MapInstituicoesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/instituicoes")
            .WithTags("Instituicoes")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllInstituicoes");

        group.MapPost("/", Create)
            .WithName("CreateInstituicao");

        return routes;
    }

    private static async Task<IResult> GetAll(
        [AsParameters] PaginationParameters pagination,
        string? search,
        bool? ativo,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving instituições - Limit: {Limit}, Offset: {Offset}, Search: {Search}, Ativo: {Ativo}, User: {UserId}",
            pagination.Limit, pagination.Offset, search ?? "none", ativo?.ToString() ?? "none", userId);

        var query = new GetInstituicoesQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            search,
            ativo);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Instituições retrieved - Returned: {Count} of {Total}",
            result.Items.Count, result.TotalCount);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateInstituicaoDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Creating instituição - CNPJ: {Cnpj}, RazaoSocial: {RazaoSocial}, CreatedBy: {UserId}",
            dto.Cnpj, dto.RazaoSocial, userId);

        var command = new CreateInstituicaoCommand(
            dto.Cnpj,
            dto.RazaoSocial,
            dto.NomeFantasia,
            dto.Ativo);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Instituição created - Id: {InstituicaoId}", result.Id);

        return Results.Created($"/api/v1/instituicoes/{result.Id}", result);
    }
}
