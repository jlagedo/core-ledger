using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade Fundo.
/// </summary>
public record FundDto(
    int Id,
    string Code,
    string Name,
    string BaseCurrency,
    DateTime InceptionDate,
    ValuationFrequency ValuationFrequency,
    string ValuationFrequencyDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
///     DTO para criar um novo fundo.
/// </summary>
public record CreateFundDto
{
    [Required]
    [MaxLength(10)]
    [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "O código do fundo deve conter apenas caracteres alfanuméricos (A-Z, 0-9)")]
    public string Code { get; init; } = string.Empty;

    [Required] [MaxLength(200)] public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; init; } = string.Empty;

    [Required] public DateTime InceptionDate { get; init; }

    [Required] public ValuationFrequency ValuationFrequency { get; init; }
}

/// <summary>
///     DTO para atualizar um Fundo existente.
/// </summary>
public record UpdateFundDto
{
    [Required]
    [MaxLength(10)]
    [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "O código do fundo deve conter apenas caracteres alfanuméricos (A-Z, 0-9)")]
    public string Code { get; init; } = string.Empty;

    [Required] [MaxLength(200)] public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; init; } = string.Empty;

    [Required] public DateTime InceptionDate { get; init; }

    [Required] public ValuationFrequency ValuationFrequency { get; init; }
}