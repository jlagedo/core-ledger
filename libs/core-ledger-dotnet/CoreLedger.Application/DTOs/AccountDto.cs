using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Objeto de transferência de dados para a entidade Conta.
/// </summary>
public record AccountDto(
    int Id,
    long Code,
    string Name,
    int TypeId,
    string TypeDescription,
    AccountStatus Status,
    string StatusDescription,
    NormalBalance NormalBalance,
    string NormalBalanceDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeactivatedAt
);

/// <summary>
///     DTO para criar uma nova Conta.
/// </summary>
public record CreateAccountDto(
    long Code,
    string Name,
    int TypeId,
    AccountStatus Status,
    NormalBalance NormalBalance
);

/// <summary>
///     DTO para atualizar uma Conta existente.
/// </summary>
public record UpdateAccountDto(
    long Code,
    string Name,
    int TypeId,
    AccountStatus Status,
    NormalBalance NormalBalance
);