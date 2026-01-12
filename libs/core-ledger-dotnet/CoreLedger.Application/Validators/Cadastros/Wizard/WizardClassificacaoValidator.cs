using CoreLedger.Application.DTOs.Wizard;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para a seção de classificação do wizard.
/// </summary>
public class WizardClassificacaoValidator : AbstractValidator<WizardClassificacaoDto>
{
    public WizardClassificacaoValidator()
    {
        RuleFor(x => x.ClassificacaoCvm)
            .IsInEnum()
            .WithMessage("Classificação CVM inválida.");

        RuleFor(x => x.PublicoAlvo)
            .IsInEnum()
            .WithMessage("Público-alvo inválido.");

        RuleFor(x => x.Tributacao)
            .IsInEnum()
            .WithMessage("Tributação inválida.");

        RuleFor(x => x.ClassificacaoAnbima)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.ClassificacaoAnbima))
            .WithMessage("Classificação ANBIMA deve ter no máximo 50 caracteres.");

        RuleFor(x => x.CodigoAnbima)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoAnbima))
            .WithMessage("Código ANBIMA deve ter no máximo 20 caracteres.");
    }
}
