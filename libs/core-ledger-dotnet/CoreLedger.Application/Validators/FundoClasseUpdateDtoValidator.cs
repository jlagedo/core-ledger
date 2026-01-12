using CoreLedger.Application.DTOs.FundoClasse;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for FundoClasseUpdateDto.
/// </summary>
public class FundoClasseUpdateDtoValidator : AbstractValidator<FundoClasseUpdateDto>
{
    public FundoClasseUpdateDtoValidator()
    {
        RuleFor(x => x.NomeClasse)
            .NotEmpty()
            .WithMessage("Nome da classe é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome da classe deve ter no máximo 100 caracteres");

        RuleFor(x => x.CnpjClasse)
            .Matches("^[0-9]{14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.CnpjClasse))
            .WithMessage("CNPJ da classe deve conter exatamente 14 dígitos numéricos");

        RuleFor(x => x.OrdemSubordinacao)
            .GreaterThan(0)
            .When(x => x.OrdemSubordinacao.HasValue)
            .WithMessage("Ordem de subordinação deve ser maior que zero");

        RuleFor(x => x.RentabilidadeAlvo)
            .InclusiveBetween(0, 1000)
            .When(x => x.RentabilidadeAlvo.HasValue)
            .WithMessage("Rentabilidade alvo deve estar entre 0 e 1000%");

        RuleFor(x => x.ValorMinimoAplicacao)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimoAplicacao.HasValue)
            .WithMessage("Valor mínimo de aplicação não pode ser negativo");
    }
}
