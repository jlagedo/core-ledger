using CoreLedger.Application.DTOs.Wizard;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;

/// <summary>
///     Handler para criação de fundo via wizard.
///     Cria o fundo com todas as entidades relacionadas em uma única transação atômica.
/// </summary>
public class CriarFundoWizardCommandHandler : IRequestHandler<CriarFundoWizardCommand, FundoWizardResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CriarFundoWizardCommandHandler> _logger;

    public CriarFundoWizardCommandHandler(
        IApplicationDbContext context,
        ILogger<CriarFundoWizardCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FundoWizardResponseDto> Handle(
        CriarFundoWizardCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation(
            "Iniciando criação de fundo via wizard - CNPJ: {Cnpj}, RazaoSocial: {RazaoSocial}, TipoFundo: {TipoFundo}",
            request.Identificacao.Cnpj,
            request.Identificacao.RazaoSocial,
            request.Identificacao.TipoFundo);

        // Begin transaction for atomicity
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Resolve all vínculo CNPJs to InstituicaoIds
            var instituicaoMap = await ResolveInstituicoesAsync(request.Vinculos, cancellationToken);

            // 2. Create main Fundo entity
            var fundo = Fundo.Criar(
                request.Identificacao.Cnpj,
                request.Identificacao.RazaoSocial,
                request.Identificacao.TipoFundo,
                request.Classificacao.ClassificacaoCvm,
                request.Caracteristicas.Prazo,
                request.Classificacao.PublicoAlvo,
                request.Classificacao.Tributacao,
                request.Caracteristicas.Condominio,
                request.Identificacao.NomeFantasia,
                request.Identificacao.NomeCurto,
                request.Identificacao.DataConstituicao,
                request.Identificacao.DataInicioAtividade,
                request.Classificacao.ClassificacaoAnbima,
                request.Classificacao.CodigoAnbima,
                request.Caracteristicas.Exclusivo,
                request.Caracteristicas.Reservado,
                request.Caracteristicas.PermiteAlavancagem,
                request.Caracteristicas.AceitaCripto,
                request.Caracteristicas.PercentualExterior,
                command.CreatedBy);

            _context.Fundos.Add(fundo);

            _logger.LogDebug("Fundo entity created with Id: {FundoId}", fundo.Id);

            // 3. Create FundoParametrosCota
            var parametrosCota = FundoParametrosCota.Criar(
                fundo.Id,
                request.ParametrosCota.TipoCota,
                request.ParametrosCota.HorarioCorte,
                request.ParametrosCota.CotaInicial,
                request.ParametrosCota.DataCotaInicial,
                request.ParametrosCota.CasasDecimaisCota,
                request.ParametrosCota.CasasDecimaisQuantidade,
                request.ParametrosCota.CasasDecimaisPl,
                request.ParametrosCota.FusoHorario,
                request.ParametrosCota.PermiteCotaEstimada);

            _context.FundoParametrosCota.Add(parametrosCota);

            _logger.LogDebug("FundoParametrosCota created");

            // 4. Create FundoTaxas
            foreach (var taxaDto in request.Taxas)
            {
                var taxa = FundoTaxa.Criar(
                    fundo.Id,
                    taxaDto.TipoTaxa,
                    taxaDto.Percentual,
                    taxaDto.BaseCalculo,
                    PeriodicidadeProvisao.Diaria, // Default para wizard
                    taxaDto.FormaCobranca,
                    taxaDto.DataInicioVigencia,
                    taxaDto.ClasseId);

                _context.FundoTaxas.Add(taxa);
            }

            _logger.LogDebug("Created {Count} FundoTaxas", request.Taxas.Count);

            // 5. Create FundoPrazos
            foreach (var prazoDto in request.Prazos)
            {
                var prazo = FundoPrazo.Criar(
                    fundo.Id,
                    prazoDto.TipoOperacao,
                    prazoDto.PrazoCotizacao,
                    prazoDto.PrazoLiquidacao,
                    new TimeOnly(14, 0), // Default horário de corte
                    true, // dias úteis por padrão
                    prazoDto.ClasseId,
                    prazoDto.PrazoCarenciaDias,
                    null, // calendarioId
                    prazoDto.PermiteResgateTotal,
                    null, // percentualMinimo
                    prazoDto.ValorMinimoInicial,
                    prazoDto.TipoCalendario,
                    prazoDto.PermiteResgateProgramado,
                    prazoDto.PrazoMaximoProgramacao);

                _context.FundoPrazos.Add(prazo);
            }

            _logger.LogDebug("Created {Count} FundoPrazos", request.Prazos.Count);

            // 6. Create FundoVinculos (using resolved InstituicaoIds)
            foreach (var vinculoDto in request.Vinculos)
            {
                var cnpjNormalizado = CNPJ.Criar(vinculoDto.CnpjInstituicao).Valor;
                var instituicaoId = instituicaoMap[cnpjNormalizado];

                var vinculo = FundoVinculo.Criar(
                    fundo.Id,
                    instituicaoId,
                    vinculoDto.TipoVinculo,
                    vinculoDto.DataInicio,
                    true); // principal = true for required vinculos

                _context.FundoVinculos.Add(vinculo);
            }

            _logger.LogDebug("Created {Count} FundoVinculos", request.Vinculos.Count);

            // 7. Create FundoClasses (if FIDC or classes provided)
            if (request.Classes != null && request.Classes.Count > 0)
            {
                foreach (var classeDto in request.Classes)
                {
                    var classe = FundoClasse.Criar(
                        fundo.Id,
                        classeDto.CodigoClasse,
                        classeDto.NomeClasse,
                        request.Identificacao.TipoFundo,
                        classeDto.CnpjClasse,
                        classeDto.TipoClasseFidc,
                        classeDto.OrdemSubordinacao,
                        classeDto.RentabilidadeAlvo,
                        classeDto.ResponsabilidadeLimitada,
                        classeDto.SegregacaoPatrimonial,
                        classeDto.ValorMinimoAplicacao,
                        classeDto.PermiteResgateAntecipado);

                    _context.FundoClasses.Add(classe);
                }

                _logger.LogDebug("Created {Count} FundoClasses", request.Classes.Count);
            }

            // 8. Create FundoParametrosFIDC (if FIDC)
            if (request.ParametrosFidc != null)
            {
                var paramsFidc = FundoParametrosFIDC.Criar(
                    fundo.Id,
                    request.ParametrosFidc.TipoFidc,
                    request.ParametrosFidc.TiposRecebiveis,
                    request.ParametrosFidc.PrazoMedioCarteira,
                    request.ParametrosFidc.IndiceSubordinacaoAlvo,
                    null, // indiceSubordinacaoMinimo
                    request.ParametrosFidc.ProvisaoDevedoresDuvidosos,
                    request.ParametrosFidc.LimiteConcentracaoCedente,
                    request.ParametrosFidc.LimiteConcentracaoSacado,
                    request.ParametrosFidc.PossuiCoobrigacao,
                    request.ParametrosFidc.PercentualCoobrigacao,
                    request.ParametrosFidc.RegistradoraRecebiveis,
                    request.ParametrosFidc.IntegracaoRegistradora,
                    request.ParametrosFidc.ContaRegistradora);

                _context.FundoParametrosFIDC.Add(paramsFidc);

                _logger.LogDebug("FundoParametrosFIDC created");
            }

            // 9. Mark wizard as complete
            fundo.AtualizarProgresso(100, command.CreatedBy);

            // 10. Save all changes
            await _context.SaveChangesAsync(cancellationToken);

            // 11. Commit transaction
            await transaction.CommitAsync(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Fundo criado via wizard com sucesso - Id: {FundoId}, CNPJ: {Cnpj}, " +
                "Classes: {ClassesCount}, Taxas: {TaxasCount}, Prazos: {PrazosCount}, " +
                "Vínculos: {VinculosCount}, Tempo: {ElapsedMs}ms",
                fundo.Id,
                request.Identificacao.Cnpj,
                request.Classes?.Count ?? 0,
                request.Taxas.Count,
                request.Prazos.Count,
                request.Vinculos.Count,
                stopwatch.ElapsedMilliseconds);

            // Return response
            return new FundoWizardResponseDto(
                fundo.Id,
                fundo.Cnpj.Formatado,
                fundo.RazaoSocial,
                fundo.NomeFantasia,
                fundo.TipoFundo,
                fundo.Situacao,
                fundo.CreatedAt);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Erro ao criar fundo via wizard - CNPJ: {Cnpj}, Erro: {Error}",
                request.Identificacao.Cnpj,
                ex.Message);

            throw;
        }
    }

    /// <summary>
    ///     Resolve all vínculo CNPJs to InstituicaoIds.
    /// </summary>
    private async Task<Dictionary<string, int>> ResolveInstituicoesAsync(
        List<WizardVinculoDto> vinculos,
        CancellationToken cancellationToken)
    {
        // Get all unique CNPJs from vinculos
        var cnpjsNormalizados = vinculos
            .Select(v => CNPJ.Criar(v.CnpjInstituicao))
            .DistinctBy(c => c.Valor)
            .ToList();

        // Query database for matching institutions
        var instituicoes = await _context.Instituicoes
            .AsNoTracking()
            .Where(i => cnpjsNormalizados.Contains(i.Cnpj) && i.Ativo)
            .Select(i => new { CnpjValor = i.Cnpj.Valor, i.Id })
            .ToListAsync(cancellationToken);

        // Build map
        var instituicaoMap = instituicoes.ToDictionary(i => i.CnpjValor, i => i.Id);

        // Validate all CNPJs were found
        var missingCnpjs = cnpjsNormalizados
            .Where(c => !instituicaoMap.ContainsKey(c.Valor))
            .Select(c => c.Formatado)
            .ToList();

        if (missingCnpjs.Count > 0)
        {
            throw new DomainValidationException(
                $"Instituições não encontradas para os CNPJs: {string.Join(", ", missingCnpjs)}");
        }

        return instituicaoMap;
    }
}
