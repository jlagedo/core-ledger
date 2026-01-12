using CoreLedger.Application.UseCases.Calendario.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for UpdateCalendarioCommand.
/// </summary>
public class UpdateCalendarioCommandValidator : AbstractValidator<UpdateCalendarioCommand>
{
    public UpdateCalendarioCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id deve ser maior que 0");

        RuleFor(x => x.TipoDia)
            .IsInEnum().WithMessage("TipoDia deve ser um valor de enum válido");

        RuleFor(x => x.Descricao)
            .MaximumLength(100).WithMessage("Descricao não pode exceder 100 caracteres");
    }
}
