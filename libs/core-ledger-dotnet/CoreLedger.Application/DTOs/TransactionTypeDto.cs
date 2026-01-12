namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade TipoTransação.
/// </summary>
public record TransactionTypeDto(
    int Id,
    string ShortDescription,
    string LongDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt);