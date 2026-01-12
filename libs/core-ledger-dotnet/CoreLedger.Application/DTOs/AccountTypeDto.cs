namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade TipoConta.
/// </summary>
public record AccountTypeDto(
    int Id,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
///     DTO para criar um novo TipoConta.
/// </summary>
public record CreateAccountTypeDto(string Description);

/// <summary>
///     DTO para atualizar um TipoConta existente.
/// </summary>
public record UpdateAccountTypeDto(string Description);