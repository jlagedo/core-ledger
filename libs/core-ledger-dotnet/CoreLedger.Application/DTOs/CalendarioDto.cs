using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     DTO para a entidade Calendário.
/// </summary>
public record CalendarioDto(
    int Id,
    DateOnly Data,
    bool DiaUtil,
    TipoDia TipoDia,
    string TipoDiaDescricao,
    Praca Praca,
    string PracaDescricao,
    string? Descricao,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
///     DTO para criar uma nova entrada de Calendário.
/// </summary>
public record CreateCalendarioDto(
    DateOnly Data,
    TipoDia TipoDia,
    Praca Praca,
    string? Descricao
);

/// <summary>
///     DTO para atualizar uma entrada de Calendário existente.
/// </summary>
public record UpdateCalendarioDto(
    TipoDia TipoDia,
    string? Descricao
);

/// <summary>
///     DTO para o resultado da verificação de dia útil.
/// </summary>
public record DiaUtilResultDto(
    DateOnly Data,
    bool DiaUtil,
    TipoDia TipoDia,
    string? Descricao
);

/// <summary>
///     DTO para o resultado do cálculo D+X.
/// </summary>
public record CalculoDMaisResultDto(
    DateOnly DataInicial,
    int DiasUteis,
    DateOnly DataFinal,
    Praca Praca
);

/// <summary>
///     DTO para o resultado da importação do calendário.
/// </summary>
public record ImportarCalendarioResultDto(
    int Ano,
    int DiasImportados,
    int DiasAtualizados
);

/// <summary>
///     DTO para o resultado da verificação de saúde do calendário (CAL-002/CAL-003).
/// </summary>
public record CalendarioHealthDto(
    bool NacionalPreenchido,
    bool Proximo30DiasOk,
    List<string> Alertas
);
