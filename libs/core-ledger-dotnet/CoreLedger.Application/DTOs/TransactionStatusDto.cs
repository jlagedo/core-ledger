namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade StatusTransação.
/// </summary>
public record TransactionStatusDto(
    int Id,
    string ShortDescription,
    string LongDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt);