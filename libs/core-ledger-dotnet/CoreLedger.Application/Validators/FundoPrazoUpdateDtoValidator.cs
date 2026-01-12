using CoreLedger.Application.DTOs.FundoPrazo;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for FundoPrazoUpdateDto.
/// </summary>
public class FundoPrazoUpdateDtoValidator : AbstractValidator<FundoPrazoUpdateDto>
{
    public FundoPrazoUpdateDtoValidator()
    {
        RuleFor(x => x.DiasCotizacao)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Dias de cotização não pode ser negativo")
            .LessThanOrEqualTo(365)
            .WithMessage("Dias de cotização deve ser no máximo 365");

        RuleFor(x => x.DiasLiquidacao)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Dias de liquidação não pode ser negativo")
            .LessThanOrEqualTo(365)
            .WithMessage("Dias de liquidação deve ser no máximo 365");

        RuleFor(x => x.HorarioLimite)
            .NotEmpty()
            .WithMessage("Horário limite é obrigatório");

        RuleFor(x => x.DiasCarencia)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DiasCarencia.HasValue)
            .WithMessage("Dias de carência não pode ser negativo");

        RuleFor(x => x.PercentualMinimo)
            .InclusiveBetween(0, 100)
            .When(x => x.PercentualMinimo.HasValue)
            .WithMessage("Percentual mínimo deve estar entre 0 e 100");

        RuleFor(x => x.ValorMinimo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimo.HasValue)
            .WithMessage("Valor mínimo não pode ser negativo");
    }
}
