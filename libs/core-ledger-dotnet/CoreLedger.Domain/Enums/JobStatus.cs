namespace CoreLedger.Domain.Enums;

/// <summary>
///     Representa o status de um trabalho principal.
/// </summary>
public enum JobStatus
{
    New = 1,
    Running = 2,
    Complete = 3,
    Failed = 4
}