using CoreLedger.Application.DTOs.FundoTaxa;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for FundoTaxaUpdateDto.
/// </summary>
public class FundoTaxaUpdateDtoValidator : AbstractValidator<FundoTaxaUpdateDto>
{
    public FundoTaxaUpdateDtoValidator()
    {
        RuleFor(x => x.Percentual)
            .InclusiveBetween(0, 100)
            .WithMessage("Percentual deve estar entre 0 e 100");

        RuleFor(x => x.BaseCalculo)
            .IsInEnum()
            .WithMessage("Base de cálculo inválida");

        RuleFor(x => x.PeriodicidadeProvisao)
            .IsInEnum()
            .WithMessage("Periodicidade de provisão inválida");

        RuleFor(x => x.PeriodicidadePagamento)
            .IsInEnum()
            .WithMessage("Periodicidade de pagamento inválida");

        RuleFor(x => x.DiaPagamento)
            .InclusiveBetween(1, 28)
            .When(x => x.DiaPagamento.HasValue)
            .WithMessage("Dia de pagamento deve estar entre 1 e 28");

        RuleFor(x => x.ValorMinimo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimo.HasValue)
            .WithMessage("Valor mínimo não pode ser negativo");

        RuleFor(x => x.ValorMaximo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMaximo.HasValue)
            .WithMessage("Valor máximo não pode ser negativo");

        RuleFor(x => x.ValorMaximo)
            .GreaterThanOrEqualTo(x => x.ValorMinimo)
            .When(x => x.ValorMinimo.HasValue && x.ValorMaximo.HasValue)
            .WithMessage("Valor máximo não pode ser menor que valor mínimo");
    }
}
