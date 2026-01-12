using System.ComponentModel.DataAnnotations;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for HistoricoIndexador entity.
/// </summary>
public record HistoricoIndexadorDto(
    long Id,
    int IndexadorId,
    string IndexadorCodigo,
    DateTime DataReferencia,
    decimal Valor,
    decimal? FatorDiario,
    decimal? VariacaoPercentual,
    string? Fonte,
    Guid? ImportacaoId,
    DateTime CreatedAt
);

/// <summary>
///     DTO for creating a new historico indexador record.
/// </summary>
public record CreateHistoricoIndexadorDto
{
    [Required] public int IndexadorId { get; init; }

    [Required] public DateTime DataReferencia { get; init; }

    [Required] public decimal Valor { get; init; }

    public decimal? FatorDiario { get; init; }

    public decimal? VariacaoPercentual { get; init; }

    [MaxLength(50)] public string? Fonte { get; init; }

    public Guid? ImportacaoId { get; init; }
}
