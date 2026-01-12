using CoreLedger.Application.DTOs.Fundo;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Query to get a fundo by its ID.
/// </summary>
public record GetFundoByIdQuery(Guid Id) : IRequest<FundoResponseDto>;
