using CoreLedger.Application.DTOs.Wizard;
using CoreLedger.Domain.Cadastros.ValueObjects;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para a seção de identificação do wizard.
/// </summary>
public class WizardIdentificacaoValidator : AbstractValidator<WizardIdentificacaoDto>
{
    public WizardIdentificacaoValidator()
    {
        RuleFor(x => x.Cnpj)
            .NotEmpty()
            .WithMessage("CNPJ é obrigatório.")
            .Must(BeValidCnpj)
            .WithMessage("CNPJ inválido.");

        RuleFor(x => x.RazaoSocial)
            .NotEmpty()
            .WithMessage("Razão social é obrigatória.")
            .MinimumLength(10)
            .WithMessage("Razão social deve ter no mínimo 10 caracteres.")
            .MaximumLength(200)
            .WithMessage("Razão social deve ter no máximo 200 caracteres.");

        RuleFor(x => x.TipoFundo)
            .IsInEnum()
            .WithMessage("Tipo de fundo inválido.");

        RuleFor(x => x.DataConstituicao)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data de constituição não pode ser futura.");

        RuleFor(x => x.DataInicioAtividade)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data de início de atividade não pode ser futura.")
            .GreaterThanOrEqualTo(x => x.DataConstituicao)
            .WithMessage("Data de início de atividade não pode ser anterior à data de constituição.");

        RuleFor(x => x.NomeFantasia)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.NomeFantasia))
            .WithMessage("Nome fantasia deve ter no máximo 100 caracteres.");

        RuleFor(x => x.NomeCurto)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.NomeCurto))
            .WithMessage("Nome curto deve ter no máximo 50 caracteres.");
    }

    private static bool BeValidCnpj(string cnpj)
    {
        return CNPJ.TentarCriar(cnpj, out _);
    }
}
