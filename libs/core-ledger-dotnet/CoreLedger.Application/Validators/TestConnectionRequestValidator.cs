using CoreLedger.Application.DTOs;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for TestConnectionRequest DTO.
/// </summary>
public class TestConnectionRequestValidator : AbstractValidator<TestConnectionRequest>
{
    public TestConnectionRequestValidator()
    {
        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .WithMessage("ReferenceId é obrigatório")
            .MaximumLength(50)
            .WithMessage("ReferenceId não pode exceder 50 caracteres");

        RuleFor(x => x.JobDescription)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.JobDescription))
            .WithMessage("JobDescription não pode exceder 255 caracteres");
    }
}