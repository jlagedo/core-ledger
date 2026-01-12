using CoreLedger.Application.DTOs.Fundo;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Query to check if a CNPJ is available for fund registration.
/// </summary>
public record VerificarCnpjDisponivelQuery(string Cnpj) : IRequest<CnpjDisponibilidadeResponseDto>;
