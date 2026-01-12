using CoreLedger.Application.DTOs;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for UpdateInstituicaoDto.
/// </summary>
public class UpdateInstituicaoDtoValidator : AbstractValidator<UpdateInstituicaoDto>
{
    public UpdateInstituicaoDtoValidator()
    {
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
