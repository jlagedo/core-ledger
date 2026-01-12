using CoreLedger.Application.DTOs.FundoTaxa;
using CoreLedger.Domain.Cadastros.Enums;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for FundoTaxaCreateDto.
/// </summary>
public class FundoTaxaCreateDtoValidator : AbstractValidator<FundoTaxaCreateDto>
{
    public FundoTaxaCreateDtoValidator()
    {
        RuleFor(x => x.TipoTaxa)
            .IsInEnum()
            .WithMessage("Tipo de taxa inválido");

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

        RuleFor(x => x.DataInicioVigencia)
            .NotEmpty()
            .WithMessage("Data de início da vigência é obrigatória");

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

        RuleFor(x => x.ParametrosPerformance)
            .NotNull()
            .When(x => x.TipoTaxa == TipoTaxa.Performance)
            .WithMessage("Parâmetros de performance são obrigatórios para taxa do tipo Performance");

        RuleFor(x => x.ParametrosPerformance)
            .SetValidator(new FundoTaxaPerformanceCreateDtoValidator()!)
            .When(x => x.ParametrosPerformance != null);
    }
}

/// <summary>
///     Validator for FundoTaxaPerformanceCreateDto.
/// </summary>
public class FundoTaxaPerformanceCreateDtoValidator : AbstractValidator<FundoTaxaPerformanceCreateDto>
{
    public FundoTaxaPerformanceCreateDtoValidator()
    {
        RuleFor(x => x.IndexadorId)
            .GreaterThan(0)
            .WithMessage("Indexador (benchmark) é obrigatório");

        RuleFor(x => x.PercentualBenchmark)
            .GreaterThan(0)
            .WithMessage("Percentual do benchmark deve ser maior que zero")
            .LessThanOrEqualTo(1000)
            .WithMessage("Percentual do benchmark não pode ser maior que 1000%");

        RuleFor(x => x.MetodoCalculo)
            .IsInEnum()
            .WithMessage("Método de cálculo inválido");

        RuleFor(x => x.PeriodicidadeCristalizacao)
            .IsInEnum()
            .WithMessage("Periodicidade de cristalização inválida");

        RuleFor(x => x.MesCristalizacao)
            .InclusiveBetween(1, 12)
            .When(x => x.MesCristalizacao.HasValue)
            .WithMessage("Mês de cristalização deve estar entre 1 e 12");
    }
}
