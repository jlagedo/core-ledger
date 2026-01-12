using CoreLedger.Application.DTOs.Wizard;
using CoreLedger.Domain.Cadastros.Enums;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para a seção de características do wizard.
/// </summary>
public class WizardCaracteristicasValidator : AbstractValidator<WizardCaracteristicasDto>
{
    public WizardCaracteristicasValidator()
    {
        RuleFor(x => x.Condominio)
            .IsInEnum()
            .WithMessage("Tipo de condomínio inválido.");

        RuleFor(x => x.Prazo)
            .IsInEnum()
            .WithMessage("Prazo do fundo inválido.");

        RuleFor(x => x.DataEncerramento)
            .NotNull()
            .When(x => x.Prazo == PrazoFundo.Determinado)
            .WithMessage("Data de encerramento é obrigatória para fundos com prazo determinado.");

        RuleFor(x => x.DataEncerramento)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DataEncerramento.HasValue)
            .WithMessage("Data de encerramento deve ser futura.");

        RuleFor(x => x.LimiteAlavancagem)
            .InclusiveBetween(0, 1000)
            .When(x => x.LimiteAlavancagem.HasValue)
            .WithMessage("Limite de alavancagem deve estar entre 0 e 1000%.");

        RuleFor(x => x.LimiteAlavancagem)
            .NotNull()
            .When(x => x.PermiteAlavancagem)
            .WithMessage("Limite de alavancagem é obrigatório quando alavancagem é permitida.");

        RuleFor(x => x.PercentualExterior)
            .InclusiveBetween(0, 100)
            .WithMessage("Percentual exterior deve estar entre 0 e 100.");
    }
}
