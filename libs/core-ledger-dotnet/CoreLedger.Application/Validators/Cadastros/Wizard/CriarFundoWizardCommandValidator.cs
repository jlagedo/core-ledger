using CoreLedger.Application.DTOs.Wizard;
using CoreLedger.Application.Interfaces;
using CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;
using CoreLedger.Domain.Cadastros;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Cadastros.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Validator principal para o comando de criação de fundo via wizard.
/// </summary>
public class CriarFundoWizardCommandValidator : AbstractValidator<CriarFundoWizardCommand>
{
    private readonly IApplicationDbContext _context;

    public CriarFundoWizardCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Validação das seções obrigatórias
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Dados do wizard são obrigatórios.");

        RuleFor(x => x.Request.Identificacao)
            .NotNull()
            .WithMessage("Identificação é obrigatória.")
            .SetValidator(new WizardIdentificacaoValidator()!);

        RuleFor(x => x.Request.Classificacao)
            .NotNull()
            .WithMessage("Classificação é obrigatória.")
            .SetValidator(new WizardClassificacaoValidator()!);

        RuleFor(x => x.Request.Caracteristicas)
            .NotNull()
            .WithMessage("Características são obrigatórias.")
            .SetValidator(new WizardCaracteristicasValidator()!);

        RuleFor(x => x.Request.ParametrosCota)
            .NotNull()
            .WithMessage("Parâmetros de cota são obrigatórios.")
            .SetValidator(new WizardParametrosCotaValidator()!);

        // Validação de unicidade do CNPJ
        RuleFor(x => x.Request.Identificacao.Cnpj)
            .MustAsync(BeUniqueCnpj)
            .WithMessage("CNPJ já cadastrado em outro fundo.")
            .WithErrorCode(FundoErrorCodes.FundoCnpjExists);

        // Validação de taxas
        RuleFor(x => x.Request.Taxas)
            .NotEmpty()
            .WithMessage("Pelo menos uma taxa deve ser informada.");

        RuleForEach(x => x.Request.Taxas)
            .SetValidator(new WizardTaxaValidator());

        RuleFor(x => x.Request.Taxas)
            .Must(HaveTaxaAdministracao)
            .WithMessage("Taxa de administração é obrigatória.")
            .WithErrorCode("TAXA_ADMINISTRACAO_OBRIGATORIA");

        // Validação de prazos
        RuleFor(x => x.Request.Prazos)
            .NotEmpty()
            .WithMessage("Pelo menos um prazo deve ser informado.");

        RuleForEach(x => x.Request.Prazos)
            .SetValidator(new WizardPrazoValidator());

        RuleFor(x => x.Request.Prazos)
            .Must(HavePrazoAplicacao)
            .WithMessage("Prazo de aplicação é obrigatório.")
            .WithErrorCode("PRAZO_APLICACAO_OBRIGATORIO");

        RuleFor(x => x.Request.Prazos)
            .Must(HavePrazoResgate)
            .WithMessage("Prazo de resgate é obrigatório.")
            .WithErrorCode("PRAZO_RESGATE_OBRIGATORIO");

        // Validação de vínculos
        RuleFor(x => x.Request.Vinculos)
            .NotEmpty()
            .WithMessage("Pelo menos um vínculo deve ser informado.");

        RuleForEach(x => x.Request.Vinculos)
            .SetValidator(new WizardVinculoValidator());

        RuleFor(x => x.Request.Vinculos)
            .Must(HaveVinculoAdministrador)
            .WithMessage("Vínculo com administrador é obrigatório.")
            .WithErrorCode("VINCULO_ADMINISTRADOR_OBRIGATORIO");

        RuleFor(x => x.Request.Vinculos)
            .Must(HaveVinculoGestor)
            .WithMessage("Vínculo com gestor é obrigatório.")
            .WithErrorCode("VINCULO_GESTOR_OBRIGATORIO");

        RuleFor(x => x.Request.Vinculos)
            .Must(HaveVinculoCustodiante)
            .WithMessage("Vínculo com custodiante é obrigatório.")
            .WithErrorCode("VINCULO_CUSTODIANTE_OBRIGATORIO");

        // Validação de CNPJs dos vínculos (verificar se existem na base)
        RuleFor(x => x.Request.Vinculos)
            .MustAsync(AllVinculoCnpjsExist)
            .WithMessage("Uma ou mais instituições não foram encontradas pelo CNPJ informado.")
            .WithErrorCode("INSTITUICAO_NAO_ENCONTRADA");

        // Validação condicional para FIDC
        When(x => IsFidc(x.Request.Identificacao.TipoFundo), () =>
        {
            RuleFor(x => x.Request.Classes)
                .NotEmpty()
                .WithMessage("FIDC requer pelo menos uma classe.")
                .WithErrorCode("FIDC_REQUER_CLASSES");

            RuleFor(x => x.Request.ParametrosFidc)
                .NotNull()
                .WithMessage("FIDC requer parâmetros específicos.")
                .WithErrorCode("FIDC_REQUER_PARAMETROS");
        });

        // Validação de classes (quando presentes)
        When(x => x.Request.Classes != null && x.Request.Classes.Count > 0, () =>
        {
            RuleForEach(x => x.Request.Classes!)
                .SetValidator(new WizardClasseValidator());
        });

        // Validação de parâmetros FIDC (quando presentes)
        When(x => x.Request.ParametrosFidc != null, () =>
        {
            RuleFor(x => x.Request.ParametrosFidc!)
                .SetValidator(new WizardParametrosFidcValidator());
        });
    }

    private static bool IsFidc(TipoFundo tipoFundo)
    {
        return tipoFundo is TipoFundo.FIDC or TipoFundo.FICFIDC;
    }

    private static bool HaveTaxaAdministracao(List<WizardTaxaDto> taxas)
    {
        return taxas.Any(t => t.TipoTaxa == TipoTaxa.Administracao);
    }

    private static bool HavePrazoAplicacao(List<WizardPrazoDto> prazos)
    {
        return prazos.Any(p => p.TipoOperacao == TipoPrazoOperacional.Aplicacao);
    }

    private static bool HavePrazoResgate(List<WizardPrazoDto> prazos)
    {
        return prazos.Any(p => p.TipoOperacao == TipoPrazoOperacional.Resgate);
    }

    private static bool HaveVinculoAdministrador(List<WizardVinculoDto> vinculos)
    {
        return vinculos.Any(v => v.TipoVinculo == TipoVinculoInstitucional.Administrador);
    }

    private static bool HaveVinculoGestor(List<WizardVinculoDto> vinculos)
    {
        return vinculos.Any(v => v.TipoVinculo == TipoVinculoInstitucional.Gestor);
    }

    private static bool HaveVinculoCustodiante(List<WizardVinculoDto> vinculos)
    {
        return vinculos.Any(v => v.TipoVinculo == TipoVinculoInstitucional.Custodiante);
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

    private async Task<bool> AllVinculoCnpjsExist(List<WizardVinculoDto> vinculos, CancellationToken cancellationToken)
    {
        var cnpjs = vinculos
            .Select(v => v.CnpjInstituicao)
            .Where(c => CNPJ.TentarCriar(c, out _))
            .Select(c => CNPJ.Criar(c))
            .ToList();

        if (cnpjs.Count == 0)
            return true; // Let format validation handle this

        var existingCount = await _context.Instituicoes
            .AsNoTracking()
            .Where(i => cnpjs.Contains(i.Cnpj) && i.Ativo)
            .CountAsync(cancellationToken);

        return existingCount == cnpjs.Count;
    }
}
