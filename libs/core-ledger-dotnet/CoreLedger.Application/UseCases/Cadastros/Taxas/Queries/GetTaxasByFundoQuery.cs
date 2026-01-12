using CoreLedger.Application.DTOs.FundoTaxa;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Taxas.Queries;

/// <summary>
///     Query to get taxas by fundo ID.
/// </summary>
public record GetTaxasByFundoQuery(Guid FundoId, bool IncluirInativas = false) : IRequest<IReadOnlyList<FundoTaxaListDto>>;
