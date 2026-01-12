using CoreLedger.Application.UseCases.Calendario.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateCalendarioCommand.
/// </summary>
public class CreateCalendarioCommandValidator : AbstractValidator<CreateCalendarioCommand>
{
    public CreateCalendarioCommandValidator()
    {
        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Data é obrigatória")
            .Must(BeValidDate).WithMessage("Data deve ser uma data válida entre 1900 e 2100");

        RuleFor(x => x.TipoDia)
            .IsInEnum().WithMessage("TipoDia deve ser um valor de enum válido");

        RuleFor(x => x.Praca)
            .IsInEnum().WithMessage("Praca deve ser um valor de enum válido");

        RuleFor(x => x.Descricao)
            .MaximumLength(100).WithMessage("Descricao não pode exceder 100 caracteres");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("CreatedByUserId é obrigatório")
            .MaximumLength(200).WithMessage("CreatedByUserId não pode exceder 200 caracteres");
    }

    private bool BeValidDate(DateOnly data)
    {
        return data != default && data.Year >= 1900 && data.Year <= 2100;
    }
}
