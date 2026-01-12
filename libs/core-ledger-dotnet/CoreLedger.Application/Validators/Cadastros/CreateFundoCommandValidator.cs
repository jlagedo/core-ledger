using CoreLedger.Application.Interfaces;
using CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;
using CoreLedger.Domain.Cadastros;
using CoreLedger.Domain.Cadastros.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Validators.Cadastros;

/// <summary>
///     Validator for CreateFundoCommand.
/// </summary>
public class CreateFundoCommandValidator : AbstractValidator<CreateFundoCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateFundoCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Cnpj)
            .NotEmpty()
            .WithMessage("CNPJ é obrigatório")
            .WithErrorCode(FundoErrorCodes.FundoNotFound)
            .Must(BeValidCnpj)
            .WithMessage("CNPJ inválido")
            .MustAsync(BeUniqueCnpj)
            .WithMessage("CNPJ já cadastrado em outro fundo")
            .WithErrorCode(FundoErrorCodes.FundoCnpjExists);

        RuleFor(x => x.RazaoSocial)
            .NotEmpty()
            .WithMessage("Razão social é obrigatória")
            .MinimumLength(5)
            .WithMessage("Razão social deve ter no mínimo 5 caracteres")
            .MaximumLength(200)
            .WithMessage("Razão social deve ter no máximo 200 caracteres");

        RuleFor(x => x.TipoFundo)
            .IsInEnum()
            .WithMessage("Tipo de fundo inválido");

        RuleFor(x => x.ClassificacaoCVM)
            .IsInEnum()
            .WithMessage("Classificação CVM inválida");

        RuleFor(x => x.PublicoAlvo)
            .IsInEnum()
            .WithMessage("Público-alvo inválido");

        RuleFor(x => x.Tributacao)
            .IsInEnum()
            .WithMessage("Tributação inválida");

        RuleFor(x => x.Condominio)
            .IsInEnum()
            .WithMessage("Tipo de condomínio inválido");

        RuleFor(x => x.Prazo)
            .IsInEnum()
            .WithMessage("Prazo do fundo inválido");

        RuleFor(x => x.DataConstituicao)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DataConstituicao.HasValue)
            .WithMessage("Data de constituição não pode ser futura");

        RuleFor(x => x.DataInicioAtividade)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DataInicioAtividade.HasValue)
            .WithMessage("Data de início de atividade não pode ser futura");

        RuleFor(x => x.DataInicioAtividade)
            .GreaterThanOrEqualTo(x => x.DataConstituicao!.Value)
            .When(x => x.DataConstituicao.HasValue && x.DataInicioAtividade.HasValue)
            .WithMessage("Data de início de atividade não pode ser anterior à data de constituição");

        RuleFor(x => x.PercentualExterior)
            .InclusiveBetween(0, 100)
            .WithMessage("Percentual exterior deve estar entre 0 e 100");

        RuleFor(x => x.NomeFantasia)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.NomeFantasia))
            .WithMessage("Nome fantasia deve ter no máximo 100 caracteres");

        RuleFor(x => x.NomeCurto)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.NomeCurto))
            .WithMessage("Nome curto deve ter no máximo 30 caracteres");

        RuleFor(x => x.ClassificacaoAnbima)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.ClassificacaoAnbima))
            .WithMessage("Classificação ANBIMA deve ter no máximo 50 caracteres");

        RuleFor(x => x.CodigoAnbima)
            .Matches("^[0-9]{6}$")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoAnbima))
            .WithMessage("Código ANBIMA deve conter exatamente 6 dígitos");
    }

    private static bool BeValidCnpj(string cnpj)
    {
        return CNPJ.TentarCriar(cnpj, out _);
    }

    private async Task<bool> BeUniqueCnpj(string cnpj, CancellationToken cancellationToken)
    {
        if (!CNPJ.TentarCriar(cnpj, out var cnpjVO) || cnpjVO is null)
            return true; // Let the CNPJ format validation handle this

        var exists = await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Cnpj == cnpjVO && f.DeletedAt == null, cancellationToken);

        return !exists;
    }
}
