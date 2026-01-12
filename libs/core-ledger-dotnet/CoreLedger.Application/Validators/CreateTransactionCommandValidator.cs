using CoreLedger.Application.UseCases.Transactions.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateTransactionCommand.
/// </summary>
public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.FundId)
            .GreaterThan(0)
            .WithMessage("FundId deve ser um identificador positivo válido");

        RuleFor(x => x.TransactionSubTypeId)
            .GreaterThan(0)
            .WithMessage("TransactionSubTypeId deve ser um identificador positivo válido");

        RuleFor(x => x.TradeDate)
            .LessThanOrEqualTo(x => x.SettleDate)
            .WithMessage("Data de negociação deve ser no máximo até a data de liquidação");

        RuleFor(x => x.SettleDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1))
            .WithMessage("Data de liquidação não pode ser mais de 1 ano no futuro");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Preço não pode ser negativo")
            .Must(price => Math.Abs(price) <= 9999999999.99999999m)
            .WithMessage("Preço excede a precisão máxima de decimal(18,8)");

        RuleFor(x => x.Quantity)
            .Must(quantity => Math.Abs(quantity) <= 9999999999.99999999m)
            .WithMessage("Quantidade excede a precisão máxima de decimal(18,8)");

        RuleFor(x => x.Amount)
            .Must(amount => Math.Abs(amount) <= 9999999999999999.99m)
            .WithMessage("Valor excede a precisão máxima de decimal(18,2)");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Moeda é obrigatória")
            .Length(3)
            .WithMessage("Moeda deve ter exatamente 3 caracteres")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Moeda deve ser 3 letras maiúsculas (código ISO)");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey é obrigatório");
    }
}