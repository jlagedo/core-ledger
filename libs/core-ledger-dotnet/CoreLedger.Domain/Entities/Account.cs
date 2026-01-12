using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade de domínio Conta com regras de negócio e invariantes.
/// </summary>
public class Account : BaseEntity
{
    private Account()
    {
    }

    public long Code { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int TypeId { get; private set; }
    public AccountType? Type { get; private set; }
    public AccountStatus Status { get; private set; }
    public NormalBalance NormalBalance { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    /// <summary>
    ///     Identificador do usuário que criou esta conta.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Método factory para criar uma nova Conta com validação.
    /// </summary>
    public static Account Create(
        long code,
        string name,
        int typeId,
        AccountStatus status,
        NormalBalance normalBalance,
        string createdByUserId)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateCreatedByUserId(createdByUserId);

        return new Account
        {
            Code = code,
            Name = name.Trim(),
            TypeId = typeId,
            Status = status,
            NormalBalance = normalBalance,
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Atualiza a conta com validação.
    /// </summary>
    public void Update(
        long code,
        string name,
        int typeId,
        AccountStatus status,
        NormalBalance normalBalance)
    {
        ValidateCode(code);
        ValidateName(name);

        Code = code;
        Name = name.Trim();
        TypeId = typeId;
        Status = status;
        NormalBalance = normalBalance;
        SetUpdated();
    }

    /// <summary>
    ///     Ativa a conta.
    /// </summary>
    public void Activate()
    {
        if (Status == AccountStatus.Active)
            throw new DomainValidationException("Conta já está ativa");

        Status = AccountStatus.Active;
        SetUpdated();
    }

    /// <summary>
    ///     Desativa a conta.
    /// </summary>
    public void Deactivate()
    {
        if (Status == AccountStatus.Inactive)
            throw new DomainValidationException("Conta já está inativa");

        Status = AccountStatus.Inactive;
        DeactivatedAt = DateTime.UtcNow;
        SetUpdated();
    }

    private static void ValidateCode(long code)
    {
        if (code <= 0)
            throw new DomainValidationException("Código deve ser um número positivo");

        if (code > 9999999999)
            throw new DomainValidationException("Código não pode exceder 10 dígitos");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Nome não pode estar vazio");

        if (name.Length > 200)
            throw new DomainValidationException("Nome não pode exceder 200 caracteres");
    }

    private static void ValidateCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
            throw new DomainValidationException("CreatedByUserId não pode estar vazio");
    }
}