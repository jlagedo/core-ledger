using CoreLedger.Application.DTOs;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for UpdateFundoVinculoDto.
/// </summary>
public class UpdateFundoVinculoDtoValidator : AbstractValidator<UpdateFundoVinculoDto>
{
    public UpdateFundoVinculoDtoValidator()
    {
        RuleFor(x => x.DataInicio)
            .NotEmpty()
            .WithMessage("Data de início é obrigatória")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data de início não pode ser futura");

        RuleFor(x => x.ContratoNumero)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.ContratoNumero))
            .WithMessage("Número do contrato não pode exceder 50 caracteres");

        RuleFor(x => x.Observacao)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacao))
            .WithMessage("Observação não pode exceder 500 caracteres");
    }
}
