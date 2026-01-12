using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for FundoVinculo entity with related entities.
/// </summary>
public record FundoVinculoDto(
    long Id,
    Guid FundoId,
    int InstituicaoId,
    string InstituicaoRazaoSocial,
    string InstituicaoCnpj,
    TipoVinculoInstitucional TipoVinculo,
    string TipoVinculoDescricao,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string? ContratoNumero,
    string? Observacao,
    bool Principal,
    bool EstaVigente,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
///     DTO for creating a new fundo vínculo.
/// </summary>
public record CreateFundoVinculoDto
{
    [Required] public Guid FundoId { get; init; }

    [Required] public int InstituicaoId { get; init; }

    [Required] public TipoVinculoInstitucional TipoVinculo { get; init; }

    [Required] public DateOnly DataInicio { get; init; }

    [MaxLength(50)] public string? ContratoNumero { get; init; }

    [MaxLength(500)] public string? Observacao { get; init; }

    public bool Principal { get; init; } = false;
}

/// <summary>
///     DTO for updating an existing fundo vínculo.
/// </summary>
public record UpdateFundoVinculoDto
{
    [Required] public DateOnly DataInicio { get; init; }

    [MaxLength(50)] public string? ContratoNumero { get; init; }

    [MaxLength(500)] public string? Observacao { get; init; }

    public bool Principal { get; init; } = false;
}

/// <summary>
///     DTO for encerrar (ending) a fundo vínculo.
/// </summary>
public record EncerrarVinculoDto
{
    [Required] public DateOnly DataFim { get; init; }
}
