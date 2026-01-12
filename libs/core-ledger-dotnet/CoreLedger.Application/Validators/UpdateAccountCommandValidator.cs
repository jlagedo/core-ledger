using CoreLedger.Application.UseCases.Accounts.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for UpdateAccountCommand.
/// </summary>
public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id deve ser um identificador positivo válido");

        RuleFor(x => x.Code)
            .GreaterThan(0)
            .WithMessage("Código deve ser um número positivo")
            .LessThanOrEqualTo(9999999999)
            .WithMessage("Código não pode exceder 10 dígitos");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome não pode exceder 200 caracteres");

        RuleFor(x => x.TypeId)
            .GreaterThan(0)
            .WithMessage("TypeId deve ser um identificador positivo válido");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status deve ser um valor válido de AccountStatus");

        RuleFor(x => x.NormalBalance)
            .IsInEnum()
            .WithMessage("NormalBalance deve ser um valor válido de NormalBalance");
    }
}