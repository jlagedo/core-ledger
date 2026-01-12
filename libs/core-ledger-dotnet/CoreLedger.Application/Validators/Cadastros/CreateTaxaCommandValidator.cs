using CoreLedger.Application.Interfaces;
using CoreLedger.Application.UseCases.Cadastros.Taxas.Commands;
using CoreLedger.Domain.Cadastros;
using CoreLedger.Domain.Cadastros.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Validators.Cadastros;

/// <summary>
///     Validator for CreateTaxaCommand.
/// </summary>
public class CreateTaxaCommandValidator : AbstractValidator<CreateTaxaCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateTaxaCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.FundoId)
            .NotEmpty()
            .WithMessage("FundoId é obrigatório")
            .MustAsync(FundoExists)
            .WithMessage("Fundo não encontrado ou excluído")
            .WithErrorCode(FundoErrorCodes.FundoNotFound);

        RuleFor(x => x.TipoTaxa)
            .IsInEnum()
            .WithMessage("Tipo de taxa inválido");

        RuleFor(x => x.Percentual)
            .GreaterThan(0)
            .WithMessage("Percentual deve ser maior que zero")
            .LessThanOrEqualTo(100)
            .WithMessage("Percentual não pode ser maior que 100");

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
            .WithMessage("Parâmetros de performance são obrigatórios para taxa do tipo Performance")
            .WithErrorCode(FundoErrorCodes.TaxaBenchmarkRequired)
            .When(x => x.TipoTaxa == TipoTaxa.Performance);

        RuleFor(x => x.ParametrosPerformance!.IndexadorId)
            .GreaterThan(0)
            .WithMessage("Indexador (benchmark) é obrigatório para taxa de performance")
            .WithErrorCode(FundoErrorCodes.TaxaBenchmarkRequired)
            .When(x => x.TipoTaxa == TipoTaxa.Performance && x.ParametrosPerformance != null);

        RuleFor(x => x.ParametrosPerformance!)
            .SetValidator(new CreateTaxaPerformanceParamsValidator()!)
            .When(x => x.ParametrosPerformance != null);

        RuleFor(x => x)
            .MustAsync(NotHaveDuplicateActiveTaxa)
            .WithMessage("Já existe taxa ativa do mesmo tipo para este fundo/classe")
            .WithErrorCode(FundoErrorCodes.TaxaDuplicada);

        RuleFor(x => x.ClasseId)
            .MustAsync(ClasseExistsIfProvided!)
            .WithMessage("Classe não encontrada ou excluída")
            .WithErrorCode(FundoErrorCodes.ClasseNotFound);
    }

    private async Task<bool> FundoExists(Guid fundoId, CancellationToken cancellationToken)
    {
        return await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == fundoId && f.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> ClasseExistsIfProvided(Guid? classeId, CancellationToken cancellationToken)
    {
        if (!classeId.HasValue)
            return true;

        return await _context.FundoClasses
            .AsNoTracking()
            .AnyAsync(c => c.Id == classeId.Value && c.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> NotHaveDuplicateActiveTaxa(CreateTaxaCommand command, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var query = _context.FundoTaxas
            .AsNoTracking()
            .Where(t => t.FundoId == command.FundoId &&
                        t.TipoTaxa == command.TipoTaxa &&
                        t.Ativa &&
                        (t.DataFimVigencia == null || t.DataFimVigencia >= today));

        if (command.ClasseId.HasValue)
            query = query.Where(t => t.ClasseId == command.ClasseId);
        else
            query = query.Where(t => t.ClasseId == null);

        return !await query.AnyAsync(cancellationToken);
    }
}

/// <summary>
///     Validator for performance parameters nested object.
/// </summary>
internal class CreateTaxaPerformanceParamsValidator : AbstractValidator<DTOs.FundoTaxa.FundoTaxaPerformanceCreateDto>
{
    public CreateTaxaPerformanceParamsValidator()
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
