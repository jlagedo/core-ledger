using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Cadastros.Enums;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for UpdateFundoParametrosFIDCDto.
/// </summary>
public class UpdateFundoParametrosFIDCDtoValidator : AbstractValidator<UpdateFundoParametrosFIDCDto>
{
    public UpdateFundoParametrosFIDCDtoValidator()
    {
        RuleFor(x => x.TipoFidc)
            .IsInEnum()
            .WithMessage("Tipo de FIDC deve ser um valor válido");

        RuleFor(x => x.TiposRecebiveis)
            .NotEmpty()
            .WithMessage("Pelo menos um tipo de recebível deve ser informado")
            .Must(x => x.All(r => Enum.IsDefined(typeof(TipoRecebiveis), r)))
            .WithMessage("Tipos de recebíveis devem ser valores válidos");

        RuleFor(x => x.PrazoMedioCarteira)
            .GreaterThan(0)
            .When(x => x.PrazoMedioCarteira.HasValue)
            .WithMessage("Prazo médio da carteira deve ser maior que zero");

        RuleFor(x => x.IndiceSubordinacaoAlvo)
            .InclusiveBetween(0m, 1m)
            .When(x => x.IndiceSubordinacaoAlvo.HasValue)
            .WithMessage("Índice de subordinação alvo deve estar entre 0 e 1 (0-100%)");

        RuleFor(x => x.IndiceSubordinacaoMinimo)
            .InclusiveBetween(0m, 1m)
            .When(x => x.IndiceSubordinacaoMinimo.HasValue)
            .WithMessage("Índice de subordinação mínimo deve estar entre 0 e 1 (0-100%)");

        RuleFor(x => x.ProvisaoDevedoresDuvidosos)
            .InclusiveBetween(0m, 1m)
            .When(x => x.ProvisaoDevedoresDuvidosos.HasValue)
            .WithMessage("Provisão para devedores duvidosos deve estar entre 0 e 1 (0-100%)");

        RuleFor(x => x.LimiteConcentracaoCedente)
            .InclusiveBetween(0m, 1m)
            .When(x => x.LimiteConcentracaoCedente.HasValue)
            .WithMessage("Limite de concentração por cedente deve estar entre 0 e 1 (0-100%)");

        RuleFor(x => x.LimiteConcentracaoSacado)
            .InclusiveBetween(0m, 1m)
            .When(x => x.LimiteConcentracaoSacado.HasValue)
            .WithMessage("Limite de concentração por sacado deve estar entre 0 e 1 (0-100%)");

        RuleFor(x => x.PercentualCoobrigacao)
            .NotNull()
            .GreaterThan(0m)
            .LessThanOrEqualTo(1m)
            .When(x => x.PossuiCoobrigacao)
            .WithMessage("Percentual de coobrigação é obrigatório e deve estar entre 0 e 1 (0-100%) quando possui coobrigação");

        RuleFor(x => x.RegistradoraRecebiveis)
            .NotNull()
            .When(x => x.IntegracaoRegistradora)
            .WithMessage("Registradora de recebíveis é obrigatória quando há integração");

        RuleFor(x => x.RegistradoraRecebiveis)
            .IsInEnum()
            .When(x => x.RegistradoraRecebiveis.HasValue)
            .WithMessage("Registradora de recebíveis deve ser um valor válido");

        RuleFor(x => x.CodigoRegistradora)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoRegistradora))
            .WithMessage("Código da registradora não pode exceder 50 caracteres");
    }
}
