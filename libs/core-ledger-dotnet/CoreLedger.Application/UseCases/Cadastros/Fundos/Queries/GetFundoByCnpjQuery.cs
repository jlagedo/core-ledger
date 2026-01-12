using CoreLedger.Application.DTOs.Fundo;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Query to get a fundo by its CNPJ.
/// </summary>
public record GetFundoByCnpjQuery(string Cnpj) : IRequest<FundoResponseDto>;
