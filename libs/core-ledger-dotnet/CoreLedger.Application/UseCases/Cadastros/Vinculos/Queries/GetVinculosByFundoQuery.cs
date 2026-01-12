using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Vinculos.Queries;

/// <summary>
///     Query to get vínculos by fundo ID.
/// </summary>
public record GetVinculosByFundoQuery(Guid FundoId, bool IncluirEncerrados = false) : IRequest<IReadOnlyList<FundoVinculoDto>>;
