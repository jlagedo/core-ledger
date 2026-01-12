using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade Título.
/// </summary>
public record SecurityDto(
    int Id,
    string Name,
    string Ticker,
    string? Isin,
    SecurityType Type,
    string TypeDescription,
    string Currency,
    SecurityStatus Status,
    string StatusDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeactivatedAt
);

/// <summary>
///     DTO para criar um novo Título.
/// </summary>
public record CreateSecurityDto
{
    [Required] [MaxLength(200)] public string Name { get; init; } = string.Empty;

    [Required] [MaxLength(20)] public string Ticker { get; init; } = string.Empty;

    [MaxLength(12)] public string? Isin { get; init; }

    [Required] public SecurityType Type { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; init; } = string.Empty;
}

/// <summary>
///     DTO para atualizar um Título existente.
/// </summary>
public record UpdateSecurityDto
{
    [Required] [MaxLength(200)] public string Name { get; init; } = string.Empty;

    [Required] [MaxLength(20)] public string Ticker { get; init; } = string.Empty;

    [MaxLength(12)] public string? Isin { get; init; }

    [Required] public SecurityType Type { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; init; } = string.Empty;
}