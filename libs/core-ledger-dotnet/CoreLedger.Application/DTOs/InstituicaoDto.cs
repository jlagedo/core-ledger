using System.ComponentModel.DataAnnotations;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for Instituicao entity.
/// </summary>
public record InstituicaoDto(
    int Id,
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    bool Ativo,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
///     DTO for creating a new instituição.
/// </summary>
public record CreateInstituicaoDto
{
    [Required]
    [MaxLength(14)]
    [RegularExpression("^[0-9]{14}$",
        ErrorMessage = "CNPJ deve conter exatamente 14 dígitos numéricos")]
    public string Cnpj { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string RazaoSocial { get; init; } = string.Empty;

    [MaxLength(100)] public string? NomeFantasia { get; init; }

    public bool Ativo { get; init; } = true;
}

/// <summary>
///     DTO for updating an existing instituição.
/// </summary>
public record UpdateInstituicaoDto
{
    [Required]
    [MaxLength(200)]
    public string RazaoSocial { get; init; } = string.Empty;

    [MaxLength(100)] public string? NomeFantasia { get; init; }
}
