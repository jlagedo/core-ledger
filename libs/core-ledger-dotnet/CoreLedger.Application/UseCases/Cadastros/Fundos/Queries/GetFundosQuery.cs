using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Models;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Query to get fundos with pagination, sorting, and filtering.
/// </summary>
public record GetFundosQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter = null
) : IRequest<PagedResult<FundoListDto>>;
