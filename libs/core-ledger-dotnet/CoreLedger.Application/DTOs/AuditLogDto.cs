using System.Text.Json;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade RegistroAuditoria.
/// </summary>
public record AuditLogDto(
    long Id,
    string EntityName,
    string EntityId,
    string EventType,
    string? PerformedByUserId,
    DateTime PerformedAt,
    JsonElement? DataBefore,
    JsonElement? DataAfter,
    string? CorrelationId,
    string? RequestId,
    string? Source
);
