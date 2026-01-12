using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Cadastros.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Vinculos.Commands;

/// <summary>
///     Command to create a new FundoVinculo.
/// </summary>
public record CreateVinculoCommand(
    Guid FundoId,
    int InstituicaoId,
    TipoVinculoInstitucional TipoVinculo,
    DateOnly DataInicio,
    bool Principal = false,
    string? ContratoNumero = null,
    string? Observacao = null
) : IRequest<FundoVinculoDto>;
