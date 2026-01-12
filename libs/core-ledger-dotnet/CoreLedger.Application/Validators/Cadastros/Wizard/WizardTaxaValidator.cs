using CoreLedger.Application.DTOs.Wizard;
using CoreLedger.Domain.Cadastros.Enums;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para item de taxa do wizard.
/// </summary>
public class WizardTaxaValidator : AbstractValidator<WizardTaxaDto>
{
    public WizardTaxaValidator()
    {
        RuleFor(x => x.TipoTaxa)
            .IsInEnum()
            .WithMessage("Tipo de taxa inválido.");

        RuleFor(x => x.Percentual)
            .InclusiveBetween(0, 100)
            .WithMessage("Percentual deve estar entre 0 e 100.");

        RuleFor(x => x.BaseCalculo)
            .IsInEnum()
            .WithMessage("Base de cálculo inválida.");

        RuleFor(x => x.FormaCobranca)
            .IsInEnum()
            .WithMessage("Forma de cobrança inválida.");

        RuleFor(x => x.PercentualMinimo)
            .InclusiveBetween(0, 100)
            .When(x => x.PercentualMinimo.HasValue)
            .WithMessage("Percentual mínimo deve estar entre 0 e 100.");

        RuleFor(x => x.PercentualMaximo)
            .InclusiveBetween(0, 100)
            .When(x => x.PercentualMaximo.HasValue)
            .WithMessage("Percentual máximo deve estar entre 0 e 100.");

        RuleFor(x => x.PercentualMaximo)
            .GreaterThanOrEqualTo(x => x.PercentualMinimo!.Value)
            .When(x => x.PercentualMinimo.HasValue && x.PercentualMaximo.HasValue)
            .WithMessage("Percentual máximo deve ser maior ou igual ao percentual mínimo.");

        RuleFor(x => x.DataFimVigencia)
            .GreaterThan(x => x.DataInicioVigencia)
            .When(x => x.DataFimVigencia.HasValue)
            .WithMessage("Data de fim de vigência deve ser posterior à data de início.");

        // Validações específicas para taxa de performance
        RuleFor(x => x.BenchmarkId)
            .NotNull()
            .When(x => x.TipoTaxa == TipoTaxa.Performance)
            .WithMessage("Benchmark é obrigatório para taxa de performance.");

        RuleFor(x => x.PercentualBenchmark)
            .InclusiveBetween(0, 1000)
            .When(x => x.PercentualBenchmark.HasValue)
            .WithMessage("Percentual do benchmark deve estar entre 0 e 1000.");
    }
}
