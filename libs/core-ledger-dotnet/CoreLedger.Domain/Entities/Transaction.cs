using System.Text.RegularExpressions;
using CoreLedger.Domain.Exceptions;

#pragma warning disable CS0618 // Type or member is obsolete - Fund is deprecated but still used for legacy transactions

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade de domínio Transação representando transações de negociação com regras de negócio e invariantes.
/// </summary>
public class Transaction : BaseEntity
{
    private Transaction()
    {
    }

    public int FundId { get; private set; }
    public Fund? Fund { get; private set; }
    public int? SecurityId { get; private set; }
    public Security? Security { get; private set; }
    public int TransactionSubTypeId { get; private set; }
    public TransactionSubType? TransactionSubType { get; private set; }
    public DateTime TradeDate { get; private set; }
    public DateTime SettleDate { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int StatusId { get; private set; }
    public TransactionStatus? Status { get; private set; }

    /// <summary>
    ///     Identificador do usuário que criou esta transação.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Método factory para criar uma nova Transação com validação.
    /// </summary>
    public static Transaction Create(
        int fundId,
        int? securityId,
        int transactionSubTypeId,
        DateTime tradeDate,
        DateTime settleDate,
        decimal quantity,
        decimal price,
        decimal amount,
        string currency,
        int statusId,
        string createdByUserId)
    {
        ValidateFundId(fundId);
        ValidateTransactionSubTypeId(transactionSubTypeId);
        ValidateStatusId(statusId);
        ValidateDates(tradeDate, settleDate);
        ValidateQuantity(quantity);
        ValidatePrice(price);
        ValidateAmount(amount);
        ValidateCurrency(currency);
        ValidateCreatedByUserId(createdByUserId);

        return new Transaction
        {
            FundId = fundId,
            SecurityId = securityId,
            TransactionSubTypeId = transactionSubTypeId,
            TradeDate = DateTime.SpecifyKind(tradeDate.Date, DateTimeKind.Utc),
            SettleDate = DateTime.SpecifyKind(settleDate.Date, DateTimeKind.Utc),
            Quantity = quantity,
            Price = price,
            Amount = amount,
            Currency = currency.Trim().ToUpperInvariant(),
            StatusId = statusId,
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Atualiza a transação com validação.
    /// </summary>
    public void Update(
        int fundId,
        int? securityId,
        int transactionSubTypeId,
        DateTime tradeDate,
        DateTime settleDate,
        decimal quantity,
        decimal price,
        decimal amount,
        string currency,
        int statusId)
    {
        ValidateFundId(fundId);
        ValidateTransactionSubTypeId(transactionSubTypeId);
        ValidateStatusId(statusId);
        ValidateDates(tradeDate, settleDate);
        ValidateQuantity(quantity);
        ValidatePrice(price);
        ValidateAmount(amount);
        ValidateCurrency(currency);

        FundId = fundId;
        SecurityId = securityId;
        TransactionSubTypeId = transactionSubTypeId;
        TradeDate = DateTime.SpecifyKind(tradeDate.Date, DateTimeKind.Utc);
        SettleDate = DateTime.SpecifyKind(settleDate.Date, DateTimeKind.Utc);
        Quantity = quantity;
        Price = price;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        StatusId = statusId;
        SetUpdated();
    }

    private static void ValidateFundId(int fundId)
    {
        if (fundId <= 0)
            throw new DomainValidationException("FundId deve ser um número positivo");
    }

    private static void ValidateTransactionSubTypeId(int transactionSubTypeId)
    {
        if (transactionSubTypeId <= 0)
            throw new DomainValidationException("TransactionSubTypeId deve ser um número positivo");
    }

    private static void ValidateStatusId(int statusId)
    {
        if (statusId <= 0)
            throw new DomainValidationException("StatusId deve ser um número positivo");
    }

    private static void ValidateDates(DateTime tradeDate, DateTime settleDate)
    {
        if (tradeDate > settleDate)
            throw new DomainValidationException("Data de negociação deve estar na ou antes da data de liquidação");

        if (settleDate > DateTime.UtcNow.AddYears(1))
            throw new DomainValidationException("Data de liquidação não pode estar mais de 1 ano no futuro");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new DomainValidationException("Preço não pode ser negativo");

        if (Math.Abs(price) > 9999999999.99999999m)
            throw new DomainValidationException("Preço excede o valor máximo permitido de 9.999.999.999,99999999");
    }

    private static void ValidateQuantity(decimal quantity)
    {
        if (Math.Abs(quantity) > 9999999999.99999999m)
            throw new DomainValidationException("Quantidade excede o valor máximo permitido de 9.999.999.999,99999999");
    }

    private static void ValidateAmount(decimal amount)
    {
        if (Math.Abs(amount) > 9999999999999999.99m)
            throw new DomainValidationException("Valor excede o valor máximo permitido de 9.999.999.999.999.999,99");
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