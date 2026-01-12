using CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateHistoricoIndexadorCommand with business validation rules.
/// </summary>
public class CreateHistoricoIndexadorCommandValidator : AbstractValidator<CreateHistoricoIndexadorCommand>
{
    public CreateHistoricoIndexadorCommandValidator()
    {
        RuleFor(x => x.IndexadorId)
            .GreaterThan(0)
            .WithMessage("IndexadorId deve ser um identificador positivo válido");

        RuleFor(x => x.DataReferencia)
            .NotEmpty()
            .WithMessage("DataReferencia é obrigatória");

        RuleFor(x => x.Valor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor deve ser maior ou igual a zero");

        RuleFor(x => x.FatorDiario)
            .GreaterThan(0)
            .When(x => x.FatorDiario.HasValue)
            .WithMessage("Fator diário deve ser maior que zero quando fornecido");

        RuleFor(x => x.VariacaoPercentual)
            .Must(v => !v.HasValue || v.Value >= -100)
            .WithMessage("Variação percentual deve ser maior ou igual a -100% quando fornecida");

        RuleFor(x => x.Fonte)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Fonte))
            .WithMessage("Fonte não pode exceder 50 caracteres");
    }
}
