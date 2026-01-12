using System.Text.RegularExpressions;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade de domínio Fundo com regras de negócio e invariantes.
/// </summary>
[Obsolete("DEPRECATED: Use CoreLedger.Domain.Cadastros.Entities.Fundo instead. This entity will be removed in a future version.", false)]
public class Fund : BaseEntity
{
    private Fund()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string BaseCurrency { get; private set; } = string.Empty;
    public DateTime InceptionDate { get; private set; }
    public ValuationFrequency ValuationFrequency { get; private set; }

    /// <summary>
    ///     Identificador do usuário que criou este fundo.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Método factory para criar um novo Fundo com validação.
    /// </summary>
    public static Fund Create(
        string code,
        string name,
        string baseCurrency,
        DateTime inceptionDate,
        ValuationFrequency valuationFrequency,
        string createdByUserId)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateBaseCurrency(baseCurrency);
        ValidateInceptionDate(inceptionDate);
        ValidateCreatedByUserId(createdByUserId);

        return new Fund
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            BaseCurrency = baseCurrency.Trim().ToUpperInvariant(),
            InceptionDate = inceptionDate.Date,
            ValuationFrequency = valuationFrequency,
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Atualiza o fundo com validação.
    /// </summary>
    public void Update(
        string code,
        string name,
        string baseCurrency,
        DateTime inceptionDate,
        ValuationFrequency valuationFrequency)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateBaseCurrency(baseCurrency);
        ValidateInceptionDate(inceptionDate);

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        BaseCurrency = baseCurrency.Trim().ToUpperInvariant();
        InceptionDate = inceptionDate.Date;
        ValuationFrequency = valuationFrequency;
        SetUpdated();
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainValidationException("Código do fundo não pode estar vazio");

        if (code.Length > 10)
            throw new DomainValidationException("Código do fundo não pode exceder 10 caracteres");

        if (!Regex.IsMatch(code, "^[A-Z0-9]+$", RegexOptions.IgnoreCase))
            throw new DomainValidationException("Código do fundo deve conter apenas caracteres alfanuméricos (A-Z, 0-9)");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Nome do fundo não pode estar vazio");

        if (name.Length > 200)
            throw new DomainValidationException("Nome do fundo não pode exceder 200 caracteres");
    }

    private static void ValidateBaseCurrency(string baseCurrency)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency))
            throw new DomainValidationException("Moeda base não pode estar vazia");

        if (baseCurrency.Length != 3)
            throw new DomainValidationException("Moeda base deve ser um código ISO de 3 letras");
    }

    private static void ValidateInceptionDate(DateTime inceptionDate)
    {
        if (inceptionDate > DateTime.UtcNow.Date)
            throw new DomainValidationException("Data de início não pode ser no futuro");
    }

    private static void ValidateCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
            throw new DomainValidationException("CreatedByUserId não pode estar vazio");
    }
}