using CoreLedger.Application.UseCases.Indexadores.Commands;
using CoreLedger.Domain.Enums;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateIndexadorCommand with business rules IDX-001, IDX-003, IDX-004.
/// </summary>
public class CreateIndexadorCommandValidator : AbstractValidator<CreateIndexadorCommand>
{
    public CreateIndexadorCommandValidator()
    {
        // IDX-001: Código único (validated in handler + regex here)
        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Código é obrigatório")
            .MaximumLength(20)
            .WithMessage("Código não pode exceder 20 caracteres")
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Código deve conter apenas caracteres alfanuméricos (A-Z, 0-9)");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome não pode exceder 100 caracteres");

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("Tipo deve ser um valor válido de IndexadorTipo");

        RuleFor(x => x.Periodicidade)
            .IsInEnum()
            .WithMessage("Periodicidade deve ser um valor válido de Periodicidade");

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

        // IDX-004: Periodicidade compatibility with Tipo
        RuleFor(x => x)
            .Custom((command, context) =>
            {
                if (!IsPeriodicidadeCompatibleWithTipo(command.Tipo, command.Periodicidade))
                {
                    context.AddFailure(
                        nameof(command.Periodicidade),
                        $"Periodicidade {command.Periodicidade} não é compatível com o tipo {command.Tipo}");
                }
            });
    }

    private static bool IsPeriodicidadeCompatibleWithTipo(IndexadorTipo tipo, Periodicidade periodicidade)
    {
        return tipo switch
        {
            IndexadorTipo.Juros => periodicidade == Periodicidade.Diaria,
            IndexadorTipo.Inflacao => periodicidade == Periodicidade.Mensal ||
                                      periodicidade == Periodicidade.Anual,
            IndexadorTipo.Cambio => periodicidade == Periodicidade.Diaria,
            IndexadorTipo.IndiceBolsa => periodicidade == Periodicidade.Diaria,
            IndexadorTipo.IndiceRendaFixa => periodicidade == Periodicidade.Diaria ||
                                             periodicidade == Periodicidade.Mensal,
            IndexadorTipo.Crypto => periodicidade == Periodicidade.Diaria,
            IndexadorTipo.Outro => true, // Outro permite qualquer periodicidade
            _ => false
        };
    }
}
