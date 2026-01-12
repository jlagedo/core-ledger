namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade SubtipoTransação.
/// </summary>
public record TransactionSubTypeDto(
    int Id,
    int TypeId,
    string TypeDescription,
    string ShortDescription,
    string LongDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt);