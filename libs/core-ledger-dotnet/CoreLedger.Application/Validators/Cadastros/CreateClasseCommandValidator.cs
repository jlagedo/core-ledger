using CoreLedger.Application.Interfaces;
using CoreLedger.Application.UseCases.Cadastros.Classes.Commands;
using CoreLedger.Domain.Cadastros;
using CoreLedger.Domain.Cadastros.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Validators.Cadastros;

/// <summary>
///     Validator for CreateClasseCommand.
/// </summary>
public class CreateClasseCommandValidator : AbstractValidator<CreateClasseCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateClasseCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.FundoId)
            .NotEmpty()
            .WithMessage("FundoId é obrigatório")
            .MustAsync(FundoExists)
            .WithMessage("Fundo não encontrado ou excluído")
            .WithErrorCode(FundoErrorCodes.FundoNotFound);

        RuleFor(x => x.CodigoClasse)
            .NotEmpty()
            .WithMessage("Código da classe é obrigatório")
            .MaximumLength(10)
            .WithMessage("Código da classe deve ter no máximo 10 caracteres")
            .MustAsync(BeUniqueCodigoInFundo)
            .WithMessage("Código da classe já existe neste fundo")
            .WithErrorCode(FundoErrorCodes.ClasseCodigoExists);

        RuleFor(x => x.NomeClasse)
            .NotEmpty()
            .WithMessage("Nome da classe é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome da classe deve ter no máximo 100 caracteres");

        RuleFor(x => x.CnpjClasse)
            .Matches("^[0-9]{14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.CnpjClasse))
            .WithMessage("CNPJ da classe deve conter exatamente 14 dígitos numéricos");

        RuleFor(x => x.TipoClasseFidc)
            .NotNull()
            .WithMessage("Tipo de classe FIDC é obrigatório para fundos FIDC")
            .WhenAsync(IsFundoFidc);

        RuleFor(x => x.TipoClasseFidc)
            .IsInEnum()
            .When(x => x.TipoClasseFidc.HasValue)
            .WithMessage("Tipo de classe FIDC inválido");

        RuleFor(x => x.OrdemSubordinacao)
            .NotNull()
            .WithMessage("Ordem de subordinação é obrigatória para fundos FIDC")
            .WhenAsync(IsFundoFidc);

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

    private async Task<bool> FundoExists(Guid fundoId, CancellationToken cancellationToken)
    {
        return await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == fundoId && f.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> BeUniqueCodigoInFundo(CreateClasseCommand command, string codigoClasse,
        CancellationToken cancellationToken)
    {
        return !await _context.FundoClasses
            .AsNoTracking()
            .AnyAsync(c => c.FundoId == command.FundoId &&
                           c.CodigoClasse == codigoClasse &&
                           c.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> IsFundoFidc(CreateClasseCommand command, CancellationToken cancellationToken)
    {
        var fundo = await _context.Fundos
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == command.FundoId && f.DeletedAt == null, cancellationToken);

        return fundo?.TipoFundo is TipoFundo.FIDC or TipoFundo.FICFIDC;
    }
}
