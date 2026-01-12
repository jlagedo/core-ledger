using CoreLedger.Application.DTOs.FundoClasse;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Queries;

/// <summary>
///     Query to get a classe by its ID.
/// </summary>
public record GetClasseByIdQuery(Guid Id) : IRequest<FundoClasseResponseDto>;
