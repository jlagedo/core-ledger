using CoreLedger.Application.DTOs.FundoClasse;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Queries;

/// <summary>
///     Query to get classes by fundo ID.
/// </summary>
public record GetClassesByFundoQuery(Guid FundoId) : IRequest<IReadOnlyList<FundoClasseListDto>>;
