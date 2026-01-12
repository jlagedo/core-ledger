using CoreLedger.Application.DTOs.Fundo;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Query to search fundos by text.
/// </summary>
public record SearchFundosQuery(
    string SearchTerm,
    int Limit = 20
) : IRequest<IReadOnlyList<FundoListDto>>;
