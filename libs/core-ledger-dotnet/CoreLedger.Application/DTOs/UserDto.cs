namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade Usuário.
/// </summary>
public record UserDto(
    int Id,
    string AuthProviderId,
    string Provider,
    string? Email,
    string? Name,
    DateTime LastLoginAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);