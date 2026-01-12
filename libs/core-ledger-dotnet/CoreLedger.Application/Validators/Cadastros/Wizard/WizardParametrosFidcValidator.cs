using CoreLedger.Application.DTOs.Wizard;
using FluentValidation;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator para parâmetros FIDC do wizard.
/// </summary>
public class WizardParametrosFidcValidator : AbstractValidator<WizardParametrosFidcDto>
{
    public WizardParametrosFidcValidator()
    {
        RuleFor(x => x.TipoFidc)
            .IsInEnum()
            .WithMessage("Tipo de FIDC inválido.");

        RuleFor(x => x.TiposRecebiveis)
            .NotEmpty()
            .WithMessage("Tipos de recebíveis são obrigatórios.")
            .Must(x => x.All(t => Enum.IsDefined(t)))
            .WithMessage("Tipo de recebível inválido.");

        RuleFor(x => x.PrazoMedioCarteira)
            .GreaterThan(0)
            .When(x => x.PrazoMedioCarteira.HasValue)
            .WithMessage("Prazo médio da carteira deve ser maior que zero.");

        RuleFor(x => x.IndiceSubordinacaoAlvo)
            .InclusiveBetween(0, 1)
            .When(x => x.IndiceSubordinacaoAlvo.HasValue)
            .WithMessage("Índice de subordinação alvo deve estar entre 0 e 1.");

        RuleFor(x => x.ProvisaoDevedoresDuvidosos)
            .InclusiveBetween(0, 1)
            .When(x => x.ProvisaoDevedoresDuvidosos.HasValue)
            .WithMessage("Provisão para devedores duvidosos deve estar entre 0 e 1.");

        RuleFor(x => x.LimiteConcentracaoCedente)
            .InclusiveBetween(0, 1)
            .When(x => x.LimiteConcentracaoCedente.HasValue)
            .WithMessage("Limite de concentração por cedente deve estar entre 0 e 1.");

        RuleFor(x => x.LimiteConcentracaoSacado)
            .InclusiveBetween(0, 1)
            .When(x => x.LimiteConcentracaoSacado.HasValue)
            .WithMessage("Limite de concentração por sacado deve estar entre 0 e 1.");

        RuleFor(x => x.PercentualCoobrigacao)
            .InclusiveBetween(0, 1)
            .When(x => x.PercentualCoobrigacao.HasValue)
            .WithMessage("Percentual de coobrigação deve estar entre 0 e 1.");

        RuleFor(x => x.PercentualCoobrigacao)
            .NotNull()
            .When(x => x.PossuiCoobrigacao)
            .WithMessage("Percentual de coobrigação é obrigatório quando coobrigação é habilitada.");

        RuleFor(x => x.RatingMinimo)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.RatingMinimo))
            .WithMessage("Rating mínimo deve ter no máximo 10 caracteres.");

        RuleFor(x => x.AgenciaRating)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.AgenciaRating))
            .WithMessage("Agência de rating deve ter no máximo 50 caracteres.");

        RuleFor(x => x.RegistradoraRecebiveis)
            .IsInEnum()
            .When(x => x.RegistradoraRecebiveis.HasValue)
            .WithMessage("Registradora inválida.");

        RuleFor(x => x.ContaRegistradora)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.ContaRegistradora))
            .WithMessage("Conta na registradora deve ter no máximo 50 caracteres.");
    }
}
