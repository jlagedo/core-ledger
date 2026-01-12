using CoreLedger.Application.DTOs;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateFundoVinculoDto.
/// </summary>
public class CreateFundoVinculoDtoValidator : AbstractValidator<CreateFundoVinculoDto>
{
    public CreateFundoVinculoDtoValidator()
    {
        RuleFor(x => x.FundoId)
            .NotEmpty()
            .WithMessage("FundoId é obrigatório");

        RuleFor(x => x.InstituicaoId)
            .GreaterThan(0)
            .WithMessage("InstituicaoId deve ser maior que zero");

        RuleFor(x => x.TipoVinculo)
            .IsInEnum()
            .WithMessage("Tipo de vínculo deve ser um valor válido");

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
