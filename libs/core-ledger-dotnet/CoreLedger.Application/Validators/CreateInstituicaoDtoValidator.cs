using CoreLedger.Application.DTOs;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateInstituicaoDto.
/// </summary>
public class CreateInstituicaoDtoValidator : AbstractValidator<CreateInstituicaoDto>
{
    public CreateInstituicaoDtoValidator()
    {
        RuleFor(x => x.Cnpj)
            .NotEmpty()
            .WithMessage("CNPJ é obrigatório")
            .Length(14)
            .WithMessage("CNPJ deve conter exatamente 14 dígitos")
            .Matches("^[0-9]{14}$")
            .WithMessage("CNPJ deve conter apenas dígitos numéricos");

        RuleFor(x => x.RazaoSocial)
            .NotEmpty()
            .WithMessage("Razão social é obrigatória")
            .MaximumLength(200)
            .WithMessage("Razão social não pode exceder 200 caracteres");

        RuleFor(x => x.NomeFantasia)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.NomeFantasia))
            .WithMessage("Nome fantasia não pode exceder 100 caracteres");
    }
}
