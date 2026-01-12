using CoreLedger.Application.DTOs.Wizard;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para a seção de parâmetros de cota do wizard.
/// </summary>
public class WizardParametrosCotaValidator : AbstractValidator<WizardParametrosCotaDto>
{
    public WizardParametrosCotaValidator()
    {
        RuleFor(x => x.TipoCota)
            .IsInEnum()
            .WithMessage("Tipo de cota inválido.");

        RuleFor(x => x.CotaInicial)
            .GreaterThan(0)
            .WithMessage("Cota inicial deve ser maior que zero.")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Cota inicial deve ser no máximo R$ 1.000.000,00.");

        RuleFor(x => x.CasasDecimaisCota)
            .InclusiveBetween(4, 10)
            .WithMessage("Casas decimais da cota deve estar entre 4 e 10.");

        RuleFor(x => x.CasasDecimaisQuantidade)
            .InclusiveBetween(4, 8)
            .WithMessage("Casas decimais da quantidade deve estar entre 4 e 8.");

        RuleFor(x => x.CasasDecimaisPl)
            .InclusiveBetween(2, 4)
            .WithMessage("Casas decimais do PL deve estar entre 2 e 4.");

        RuleFor(x => x.FusoHorario)
            .NotEmpty()
            .WithMessage("Fuso horário é obrigatório.")
            .MaximumLength(50)
            .WithMessage("Fuso horário deve ter no máximo 50 caracteres.");
    }
}
