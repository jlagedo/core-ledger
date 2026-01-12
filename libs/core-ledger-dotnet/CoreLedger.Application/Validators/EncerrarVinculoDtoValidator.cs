using CoreLedger.Application.DTOs;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for EncerrarVinculoDto.
/// </summary>
public class EncerrarVinculoDtoValidator : AbstractValidator<EncerrarVinculoDto>
{
    public EncerrarVinculoDtoValidator()
    {
        RuleFor(x => x.DataFim)
            .NotEmpty()
            .WithMessage("Data de fim é obrigatória")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data de fim não pode ser futura");
    }
}
