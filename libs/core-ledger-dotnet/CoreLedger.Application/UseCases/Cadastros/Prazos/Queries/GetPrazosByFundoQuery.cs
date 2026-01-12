using CoreLedger.Application.DTOs.FundoPrazo;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Prazos.Queries;

/// <summary>
///     Query to get prazos by fundo ID.
/// </summary>
public record GetPrazosByFundoQuery(Guid FundoId, bool IncluirInativos = false) : IRequest<IReadOnlyList<FundoPrazoListDto>>;
