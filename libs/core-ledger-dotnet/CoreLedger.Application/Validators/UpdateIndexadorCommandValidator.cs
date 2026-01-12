using CoreLedger.Application.UseCases.Indexadores.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for UpdateIndexadorCommand with business rule IDX-003.
///     Note: Tipo and Periodicidade are immutable after creation (IDX-004 validated at creation only).
/// </summary>
public class UpdateIndexadorCommandValidator : AbstractValidator<UpdateIndexadorCommand>
{
    public UpdateIndexadorCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id deve ser um identificador positivo válido");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome não pode exceder 100 caracteres");

        RuleFor(x => x.Fonte)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Fonte))
            .WithMessage("Fonte não pode exceder 100 caracteres");

        // FatorAcumulado requires DataBase
        RuleFor(x => x.FatorAcumulado)
            .GreaterThan(0)
            .When(x => x.FatorAcumulado.HasValue)
            .WithMessage("Fator acumulado deve ser maior que zero");

        RuleFor(x => x.DataBase)
            .NotNull()
            .When(x => x.FatorAcumulado.HasValue)
            .WithMessage("Data base é obrigatória quando fator acumulado é fornecido");

        // IDX-003: ImportacaoAutomatica requires UrlFonte
        RuleFor(x => x.UrlFonte)
            .NotEmpty()
            .When(x => x.ImportacaoAutomatica)
            .WithMessage("URL fonte é obrigatória quando importação automática está habilitada");

        RuleFor(x => x.UrlFonte)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.UrlFonte))
            .WithMessage("URL fonte não pode exceder 500 caracteres");
    }
}
