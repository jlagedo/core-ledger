using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade de domínio TipoConta com regras de negócio e invariantes.
/// </summary>
public class AccountType : BaseEntity
{
    private AccountType()
    {
    }

    public string Description { get; private set; } = string.Empty;

    /// <summary>
    ///     Método factory para criar um novo TipoConta com validação.
    /// </summary>
    public static AccountType Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Descrição não pode estar vazia");

        if (description.Length > 100)
            throw new DomainValidationException("Descrição não pode exceder 100 caracteres");

        return new AccountType
        {
            Description = description.Trim()
        };
    }

    /// <summary>
    ///     Atualiza a descrição com validação.
    /// </summary>
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Descrição não pode estar vazia");

        if (description.Length > 100)
            throw new DomainValidationException("Descrição não pode exceder 100 caracteres");

        Description = description.Trim();
        SetUpdated();
    }
}