using CoreLedger.Application.DTOs.Wizard;
using CoreLedger.Domain.Cadastros.ValueObjects;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para item de classe do wizard.
/// </summary>
public class WizardClasseValidator : AbstractValidator<WizardClasseDto>
{
    public WizardClasseValidator()
    {
        RuleFor(x => x.CodigoClasse)
            .NotEmpty()
            .WithMessage("Código da classe é obrigatório.")
            .MaximumLength(20)
            .WithMessage("Código da classe deve ter no máximo 20 caracteres.");

        RuleFor(x => x.NomeClasse)
            .NotEmpty()
            .WithMessage("Nome da classe é obrigatório.")
            .MaximumLength(100)
            .WithMessage("Nome da classe deve ter no máximo 100 caracteres.");

        RuleFor(x => x.PublicoAlvo)
            .IsInEnum()
            .WithMessage("Público-alvo inválido.");

        RuleFor(x => x.CnpjClasse)
            .Must(BeValidCnpjOrNull)
            .When(x => !string.IsNullOrWhiteSpace(x.CnpjClasse))
            .WithMessage("CNPJ da classe inválido.");

        RuleFor(x => x.Nivel)
            .InclusiveBetween(1, 2)
            .WithMessage("Nível deve ser 1 (classe) ou 2 (subclasse).");

        RuleFor(x => x.ClassePaiId)
            .NotNull()
            .When(x => x.Nivel == 2)
            .WithMessage("Classe pai é obrigatória para subclasses (nível 2).");

        RuleFor(x => x.TipoClasseFidc)
            .IsInEnum()
            .When(x => x.TipoClasseFidc.HasValue)
            .WithMessage("Tipo de classe FIDC inválido.");

        RuleFor(x => x.OrdemSubordinacao)
            .GreaterThan(0)
            .When(x => x.OrdemSubordinacao.HasValue)
            .WithMessage("Ordem de subordinação deve ser maior que zero.");

        RuleFor(x => x.RentabilidadeAlvo)
            .InclusiveBetween(0, 1000)
            .When(x => x.RentabilidadeAlvo.HasValue)
            .WithMessage("Rentabilidade alvo deve estar entre 0 e 1000%.");

        RuleFor(x => x.IndiceSubordinacaoMinimo)
            .InclusiveBetween(0, 1)
            .When(x => x.IndiceSubordinacaoMinimo.HasValue)
            .WithMessage("Índice de subordinação mínimo deve estar entre 0 e 1.");

        RuleFor(x => x.ValorMinimoAplicacao)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoAplicacao.HasValue)
            .WithMessage("Valor mínimo de aplicação não pode ser negativo.");

        RuleFor(x => x.ValorMinimoPermanencia)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoPermanencia.HasValue)
            .WithMessage("Valor mínimo de permanência não pode ser negativo.");

        RuleFor(x => x.TaxaAdministracao)
            .InclusiveBetween(0, 100)
            .When(x => x.TaxaAdministracao.HasValue)
            .WithMessage("Taxa de administração deve estar entre 0 e 100%.");

        RuleFor(x => x.TaxaGestao)
            .InclusiveBetween(0, 100)
            .When(x => x.TaxaGestao.HasValue)
            .WithMessage("Taxa de gestão deve estar entre 0 e 100%.");

        RuleFor(x => x.TaxaPerformance)
            .InclusiveBetween(0, 100)
            .When(x => x.TaxaPerformance.HasValue)
            .WithMessage("Taxa de performance deve estar entre 0 e 100%.");

        RuleFor(x => x.DataEncerramento)
            .GreaterThan(x => x.DataInicio)
            .When(x => x.DataEncerramento.HasValue)
            .WithMessage("Data de encerramento deve ser posterior à data de início.");
    }

    private static bool BeValidCnpjOrNull(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return true;

        return CNPJ.TentarCriar(cnpj, out _);
    }
}
