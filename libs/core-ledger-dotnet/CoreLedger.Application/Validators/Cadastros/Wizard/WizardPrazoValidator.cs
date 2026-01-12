using CoreLedger.Application.DTOs.Wizard;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para item de prazo do wizard.
/// </summary>
public class WizardPrazoValidator : AbstractValidator<WizardPrazoDto>
{
    public WizardPrazoValidator()
    {
        RuleFor(x => x.TipoOperacao)
            .IsInEnum()
            .WithMessage("Tipo de operação inválido.");

        RuleFor(x => x.PrazoCotizacao)
            .InclusiveBetween(0, 365)
            .WithMessage("Prazo de cotização deve estar entre 0 e 365 dias.");

        RuleFor(x => x.PrazoLiquidacao)
            .InclusiveBetween(0, 365)
            .WithMessage("Prazo de liquidação deve estar entre 0 e 365 dias.");

        RuleFor(x => x.PrazoLiquidacao)
            .GreaterThanOrEqualTo(x => x.PrazoCotizacao)
            .WithMessage("Prazo de liquidação deve ser maior ou igual ao prazo de cotização.");

        RuleFor(x => x.TipoCalendario)
            .NotEmpty()
            .WithMessage("Tipo de calendário é obrigatório.")
            .MaximumLength(20)
            .WithMessage("Tipo de calendário deve ter no máximo 20 caracteres.");

        RuleFor(x => x.ValorMinimoInicial)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoInicial.HasValue)
            .WithMessage("Valor mínimo inicial não pode ser negativo.");

        RuleFor(x => x.ValorMinimoAdicional)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoAdicional.HasValue)
            .WithMessage("Valor mínimo adicional não pode ser negativo.");

        RuleFor(x => x.ValorMinimoResgate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoResgate.HasValue)
            .WithMessage("Valor mínimo de resgate não pode ser negativo.");

        RuleFor(x => x.ValorMinimoPermanencia)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoPermanencia.HasValue)
            .WithMessage("Valor mínimo de permanência não pode ser negativo.");

        RuleFor(x => x.PrazoCarenciaDias)
            .InclusiveBetween(0, 3650)
            .When(x => x.PrazoCarenciaDias.HasValue)
            .WithMessage("Prazo de carência deve estar entre 0 e 3650 dias.");

        RuleFor(x => x.PrazoMaximoProgramacao)
            .InclusiveBetween(0, 365)
            .When(x => x.PrazoMaximoProgramacao.HasValue)
            .WithMessage("Prazo máximo de programação deve estar entre 0 e 365 dias.");

        RuleFor(x => x.PrazoMaximoProgramacao)
            .NotNull()
            .When(x => x.PermiteResgateProgramado)
            .WithMessage("Prazo máximo de programação é obrigatório quando resgate programado é permitido.");
    }
}
