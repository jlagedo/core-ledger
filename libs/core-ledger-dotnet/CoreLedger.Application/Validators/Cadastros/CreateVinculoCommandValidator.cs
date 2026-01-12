using CoreLedger.Application.Interfaces;
using CoreLedger.Application.UseCases.Cadastros.Vinculos.Commands;
using CoreLedger.Domain.Cadastros;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Validators.Cadastros;

/// <summary>
///     Validator for CreateVinculoCommand.
/// </summary>
public class CreateVinculoCommandValidator : AbstractValidator<CreateVinculoCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateVinculoCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.FundoId)
            .NotEmpty()
            .WithMessage("FundoId é obrigatório")
            .MustAsync(FundoExists)
            .WithMessage("Fundo não encontrado ou excluído")
            .WithErrorCode(FundoErrorCodes.FundoNotFound);

        RuleFor(x => x.InstituicaoId)
            .GreaterThan(0)
            .WithMessage("InstituicaoId deve ser maior que zero")
            .MustAsync(InstituicaoExists)
            .WithMessage("Instituição não encontrada")
            .WithErrorCode(FundoErrorCodes.InstituicaoNotFound);

        RuleFor(x => x.TipoVinculo)
            .IsInEnum()
            .WithMessage("Tipo de vínculo inválido");

        RuleFor(x => x.DataInicio)
            .NotEmpty()
            .WithMessage("Data de início é obrigatória")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data de início não pode ser futura");

        RuleFor(x => x.ContratoNumero)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.ContratoNumero))
            .WithMessage("Número do contrato não pode exceder 50 caracteres");

        RuleFor(x => x.Observacao)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacao))
            .WithMessage("Observação não pode exceder 500 caracteres");

        RuleFor(x => x)
            .MustAsync(NotHaveActivePrincipal)
            .When(x => x.Principal)
            .WithMessage("Já existe vínculo principal ativo do mesmo tipo para este fundo")
            .WithErrorCode(FundoErrorCodes.VinculoPrincipalExists);
    }

    private async Task<bool> FundoExists(Guid fundoId, CancellationToken cancellationToken)
    {
        return await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == fundoId && f.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> InstituicaoExists(int instituicaoId, CancellationToken cancellationToken)
    {
        return await _context.Instituicoes
            .AsNoTracking()
            .AnyAsync(i => i.Id == instituicaoId && i.Ativo, cancellationToken);
    }

    private async Task<bool> NotHaveActivePrincipal(CreateVinculoCommand command, CancellationToken cancellationToken)
    {
        return !await _context.FundoVinculos
            .AsNoTracking()
            .AnyAsync(v => v.FundoId == command.FundoId &&
                           v.TipoVinculo == command.TipoVinculo &&
                           v.Principal &&
                           v.DataFim == null, cancellationToken);
    }
}
