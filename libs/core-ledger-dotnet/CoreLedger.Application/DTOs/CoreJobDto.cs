using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade TrabalhoCore.
/// </summary>
public record CoreJobDto(
    int Id,
    string ReferenceId,
    JobStatus Status,
    string StatusDescription,
    string JobDescription,
    DateTime CreationDate,
    DateTime? RunningDate,
    DateTime? FinishedDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);