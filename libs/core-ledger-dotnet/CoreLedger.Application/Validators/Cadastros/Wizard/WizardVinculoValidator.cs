using CoreLedger.Application.DTOs.Wizard;
using CoreLedger.Domain.Cadastros.ValueObjects;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para item de vínculo do wizard.
/// </summary>
public class WizardVinculoValidator : AbstractValidator<WizardVinculoDto>
{
    public WizardVinculoValidator()
    {
        RuleFor(x => x.TipoVinculo)
            .IsInEnum()
            .WithMessage("Tipo de vínculo inválido.");

        RuleFor(x => x.CnpjInstituicao)
            .NotEmpty()
            .WithMessage("CNPJ da instituição é obrigatório.")
            .Must(BeValidCnpj)
            .WithMessage("CNPJ da instituição inválido.");

        RuleFor(x => x.NomeInstituicao)
            .NotEmpty()
            .WithMessage("Nome da instituição é obrigatório.")
            .MaximumLength(200)
            .WithMessage("Nome da instituição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.CodigoCvm)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoCvm))
            .WithMessage("Código CVM deve ter no máximo 20 caracteres.");

        RuleFor(x => x.DataFim)
            .GreaterThan(x => x.DataInicio)
            .When(x => x.DataFim.HasValue)
            .WithMessage("Data de fim deve ser posterior à data de início.");

        RuleFor(x => x.MotivoFim)
            .NotEmpty()
            .When(x => x.DataFim.HasValue)
            .WithMessage("Motivo do fim é obrigatório quando data de fim é informada.")
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.MotivoFim))
            .WithMessage("Motivo do fim deve ter no máximo 200 caracteres.");

        RuleFor(x => x.ResponsavelNome)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ResponsavelNome))
            .WithMessage("Nome do responsável deve ter no máximo 100 caracteres.");

        RuleFor(x => x.ResponsavelEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ResponsavelEmail))
            .WithMessage("E-mail do responsável inválido.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ResponsavelEmail))
            .WithMessage("E-mail deve ter no máximo 100 caracteres.");

        RuleFor(x => x.ResponsavelTelefone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.ResponsavelTelefone))
            .WithMessage("Telefone deve ter no máximo 20 caracteres.");
    }

    private static bool BeValidCnpj(string cnpj)
    {
        return CNPJ.TentarCriar(cnpj, out _);
    }
}
