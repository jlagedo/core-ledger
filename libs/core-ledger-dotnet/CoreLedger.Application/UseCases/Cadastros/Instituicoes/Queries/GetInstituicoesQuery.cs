using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Instituicoes.Queries;

/// <summary>
///     Query to get instituições with pagination, sorting, and filtering.
/// </summary>
public record GetInstituicoesQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Search = null,
    bool? Ativo = null
) : IRequest<PagedResult<InstituicaoDto>>;
