using CoreLedger.Application.Interfaces;
using CoreLedger.Application.UseCases.Cadastros.Prazos.Commands;
using CoreLedger.Domain.Cadastros;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Validators.Cadastros;

/// <summary>
///     Validator for CreatePrazoCommand.
/// </summary>
public class CreatePrazoCommandValidator : AbstractValidator<CreatePrazoCommand>
{
    private readonly IApplicationDbContext _context;

    public CreatePrazoCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.FundoId)
            .NotEmpty()
            .WithMessage("FundoId é obrigatório")
            .MustAsync(FundoExists)
            .WithMessage("Fundo não encontrado ou excluído")
            .WithErrorCode(FundoErrorCodes.FundoNotFound);

        RuleFor(x => x.TipoPrazo)
            .IsInEnum()
            .WithMessage("Tipo de prazo inválido");

        RuleFor(x => x.DiasCotizacao)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Dias de cotização não pode ser negativo")
            .LessThanOrEqualTo(365)
            .WithMessage("Dias de cotização deve ser no máximo 365");

        RuleFor(x => x.DiasLiquidacao)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Dias de liquidação não pode ser negativo")
            .LessThanOrEqualTo(365)
            .WithMessage("Dias de liquidação deve ser no máximo 365")
            .GreaterThanOrEqualTo(x => x.DiasCotizacao)
            .WithMessage("Dias de liquidação não pode ser menor que dias de cotização");

        RuleFor(x => x.HorarioLimite)
            .NotEmpty()
            .WithMessage("Horário limite é obrigatório")
            .Must(BeValidHorarioLimite)
            .WithMessage("Horário limite deve estar entre 00:00 e 23:59");

        RuleFor(x => x.DiasCarencia)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DiasCarencia.HasValue)
            .WithMessage("Dias de carência não pode ser negativo");

        RuleFor(x => x.PercentualMinimo)
            .InclusiveBetween(0, 100)
            .When(x => x.PercentualMinimo.HasValue)
            .WithMessage("Percentual mínimo deve estar entre 0 e 100");

        RuleFor(x => x.ValorMinimo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimo.HasValue)
            .WithMessage("Valor mínimo não pode ser negativo");

        RuleFor(x => x.CalendarioId)
            .MustAsync(CalendarioExistsIfProvided!)
            .WithMessage("Calendário não encontrado")
            .WithErrorCode(FundoErrorCodes.CalendarioNotFound);

        RuleFor(x => x.ClasseId)
            .MustAsync(ClasseExistsIfProvided!)
            .WithMessage("Classe não encontrada ou excluída")
            .WithErrorCode(FundoErrorCodes.ClasseNotFound);

        RuleFor(x => x)
            .MustAsync(NotHaveDuplicatePrazo)
            .WithMessage("Já existe prazo do mesmo tipo para este fundo/classe")
            .WithErrorCode(FundoErrorCodes.PrazoDuplicado);
    }

    private static bool BeValidHorarioLimite(TimeOnly horario)
    {
        return horario >= new TimeOnly(0, 0) && horario <= new TimeOnly(23, 59);
    }

    private async Task<bool> FundoExists(Guid fundoId, CancellationToken cancellationToken)
    {
        return await _context.Fundos
            .AsNoTracking()
            .AnyAsync(f => f.Id == fundoId && f.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> ClasseExistsIfProvided(Guid? classeId, CancellationToken cancellationToken)
    {
        if (!classeId.HasValue)
            return true;

        return await _context.FundoClasses
            .AsNoTracking()
            .AnyAsync(c => c.Id == classeId.Value && c.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> CalendarioExistsIfProvided(int? calendarioId, CancellationToken cancellationToken)
    {
        if (!calendarioId.HasValue)
            return true;

        return await _context.Calendarios
            .AsNoTracking()
            .AnyAsync(c => c.Id == calendarioId.Value, cancellationToken);
    }

    private async Task<bool> NotHaveDuplicatePrazo(CreatePrazoCommand command, CancellationToken cancellationToken)
    {
        var query = _context.FundoPrazos
            .AsNoTracking()
            .Where(p => p.FundoId == command.FundoId &&
                        p.TipoPrazo == command.TipoPrazo &&
                        p.Ativo);

        if (command.ClasseId.HasValue)
            query = query.Where(p => p.ClasseId == command.ClasseId);
        else
            query = query.Where(p => p.ClasseId == null);

        return !await query.AnyAsync(cancellationToken);
    }
}
