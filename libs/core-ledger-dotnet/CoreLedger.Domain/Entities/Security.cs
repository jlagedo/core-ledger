using System.Text.RegularExpressions;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade de domínio Ativo com regras de negócio e invariantes.
/// </summary>
public class Security : BaseEntity
{
    private Security()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Ticker { get; private set; } = string.Empty;
    public string? Isin { get; private set; }
    public SecurityType Type { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public SecurityStatus Status { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    /// <summary>
    ///     Identificador do usuário que criou este ativo.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Método factory para criar um novo Ativo com validação.
    /// </summary>
    public static Security Create(
        string name,
        string ticker,
        string? isin,
        SecurityType type,
        string currency,
        string createdByUserId)
    {
        ValidateName(name);
        ValidateTicker(ticker);
        ValidateIsin(isin);
        ValidateCurrency(currency);
        ValidateCreatedByUserId(createdByUserId);

        return new Security
        {
            Name = name.Trim(),
            Ticker = ticker.Trim().ToUpperInvariant(),
            Isin = isin?.Trim().ToUpperInvariant(),
            Type = type,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = SecurityStatus.Active,
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Atualiza o ativo com validação.
    /// </summary>
    public void Update(
        string name,
        string ticker,
        string? isin,
        SecurityType type,
        string currency)
    {
        ValidateName(name);
        ValidateTicker(ticker);
        ValidateIsin(isin);
        ValidateCurrency(currency);

        Name = name.Trim();
        Ticker = ticker.Trim().ToUpperInvariant();
        Isin = isin?.Trim().ToUpperInvariant();
        Type = type;
        Currency = currency.Trim().ToUpperInvariant();
        SetUpdated();
    }

    /// <summary>
    ///     Desativa o ativo.
    /// </summary>
    public void Deactivate()
    {
        if (Status == SecurityStatus.Inactive)
            throw new DomainValidationException("Ativo já está inativo");

        Status = SecurityStatus.Inactive;
        DeactivatedAt = DateTime.UtcNow;
        SetUpdated();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Nome não pode estar vazio");

        if (name.Length > 200)
            throw new DomainValidationException("Nome não pode exceder 200 caracteres");
    }

    private static void ValidateTicker(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new DomainValidationException("Ticker não pode estar vazio");

        if (ticker.Length > 20)
            throw new DomainValidationException("Ticker não pode exceder 20 caracteres");

        if (!Regex.IsMatch(ticker, "^[A-Z0-9-]+$", RegexOptions.IgnoreCase))
            throw new DomainValidationException(
                "Ticker deve conter apenas caracteres alfanuméricos e hífens (A-Z, 0-9, -)");
    }

    private static void ValidateIsin(string? isin)
    {
        if (isin != null && isin.Trim().Length > 12)
            throw new DomainValidationException("ISIN não pode exceder 12 caracteres");
    }

    private static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainValidationException("Moeda não pode estar vazia");

        if (currency.Length != 3)
            throw new DomainValidationException("Moeda deve ser um código ISO de 3 letras");

        if (!Regex.IsMatch(currency, "^[A-Z]{3}$", RegexOptions.IgnoreCase))
            throw new DomainValidationException("Moeda deve conter apenas letras (A-Z)");
    }

    private static void ValidateCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
            throw new DomainValidationException("CreatedByUserId não pode estar vazio");
    }
}