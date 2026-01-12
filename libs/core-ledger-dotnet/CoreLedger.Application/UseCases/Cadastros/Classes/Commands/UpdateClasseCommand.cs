using CoreLedger.Application.DTOs.FundoClasse;
using CoreLedger.Domain.Cadastros.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Classes.Commands;

/// <summary>
///     Command to update an existing FundoClasse.
/// </summary>
public record UpdateClasseCommand(
    Guid Id,
    string NomeClasse,
    string? CnpjClasse = null,
    TipoClasseFIDC? TipoClasseFidc = null,
    int? OrdemSubordinacao = null,
    decimal? RentabilidadeAlvo = null,
    bool ResponsabilidadeLimitada = false,
    bool SegregacaoPatrimonial = false,
    decimal? ValorMinimoAplicacao = null
) : IRequest<FundoClasseResponseDto>;
