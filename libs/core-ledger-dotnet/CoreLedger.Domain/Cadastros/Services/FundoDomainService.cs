using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Domain.Cadastros.Services;

/// <summary>
///     Serviço de domínio para validações e regras de negócio complexas do Fundo.
/// </summary>
public class FundoDomainService
{
    private readonly ILogger<FundoDomainService> _logger;

    public FundoDomainService(ILogger<FundoDomainService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Pesos para cálculo do progresso do cadastro.
    /// </summary>
    private static class ProgressoPesos
    {
        public const int DadosBasicos = 20;
        public const int Classificacao = 15;
        public const int ParametrosCota = 10;
        public const int Taxas = 15;
        public const int Prazos = 15;
        public const int VinculosObrigatorios = 20;
        public const int Documentos = 5;
    }

    /// <summary>
    ///     Vínculos obrigatórios para ativação de um fundo.
    /// </summary>
    private static readonly TipoVinculoInstitucional[] VinculosObrigatorios =
    [
        TipoVinculoInstitucional.Administrador,
        TipoVinculoInstitucional.Gestor,
        TipoVinculoInstitucional.Custodiante
    ];

    /// <summary>
    ///     Valida se o fundo pode ser ativado.
    /// </summary>
    /// <param name="fundo">Fundo a ser validado.</param>
    /// <param name="classes">Classes do fundo.</param>
    /// <param name="taxas">Taxas do fundo.</param>
    /// <param name="prazos">Prazos do fundo.</param>
    /// <param name="vinculos">Vínculos do fundo.</param>
    /// <returns>True se pode ser ativado, false caso contrário.</returns>
    public bool PodeAtivar(
        Fundo fundo,
        IEnumerable<FundoClasse> classes,
        IEnumerable<FundoTaxa> taxas,
        IEnumerable<FundoPrazo> prazos,
        IEnumerable<FundoVinculo> vinculos)
    {
        _logger.LogDebug("Validando se fundo {FundoId} pode ser ativado", fundo.Id);

        // 1. Fundo deve estar em constituição
        if (fundo.Situacao != SituacaoFundo.EmConstituicao)
        {
            _logger.LogWarning(
                "Fundo {FundoId} não pode ser ativado - Situação inválida: {Situacao} (esperado: {SituacaoEsperada})",
                fundo.Id, fundo.Situacao, SituacaoFundo.EmConstituicao);
            return false;
        }

        // 2. Deve ter pelo menos uma classe ativa
        var classesAtivas = classes.Where(c => c.DeletedAt == null).ToList();
        if (classesAtivas.Count == 0)
        {
            _logger.LogWarning(
                "Fundo {FundoId} não pode ser ativado - Nenhuma classe ativa encontrada",
                fundo.Id);
            return false;
        }

        // 3. Deve ter taxa de administração configurada
        var taxasAtivas = taxas.Where(t => t.Ativa && t.DataFimVigencia == null).ToList();
        if (!taxasAtivas.Any(t => t.TipoTaxa == TipoTaxa.Administracao))
        {
            _logger.LogWarning(
                "Fundo {FundoId} não pode ser ativado - Taxa de administração não configurada",
                fundo.Id);
            return false;
        }

        // 4. Deve ter prazos de aplicação e resgate configurados
        var prazosAtivos = prazos.Where(p => p.Ativo).ToList();
        var temAplicacao = prazosAtivos.Any(p => p.TipoPrazo == TipoPrazoOperacional.Aplicacao);
        var temResgate = prazosAtivos.Any(p => p.TipoPrazo == TipoPrazoOperacional.Resgate);
        if (!temAplicacao || !temResgate)
        {
            _logger.LogWarning(
                "Fundo {FundoId} não pode ser ativado - Prazos incompletos (Aplicação: {TemAplicacao}, Resgate: {TemResgate})",
                fundo.Id, temAplicacao, temResgate);
            return false;
        }

        // 5. Deve ter vínculos obrigatórios
        var vinculosVigentes = vinculos.Where(v => v.EstaVigente()).ToList();
        var vinculosFaltantes = new List<TipoVinculoInstitucional>();
        foreach (var tipoObrigatorio in VinculosObrigatorios)
        {
            if (!vinculosVigentes.Any(v => v.TipoVinculo == tipoObrigatorio))
            {
                vinculosFaltantes.Add(tipoObrigatorio);
            }
        }

        if (vinculosFaltantes.Count > 0)
        {
            _logger.LogWarning(
                "Fundo {FundoId} não pode ser ativado - Vínculos obrigatórios faltantes: {VinculosFaltantes}",
                fundo.Id, string.Join(", ", vinculosFaltantes));
            return false;
        }

        _logger.LogInformation(
            "Fundo {FundoId} pode ser ativado - Todas as validações passaram",
            fundo.Id);

        return true;
    }

    /// <summary>
    ///     Valida se o fundo pode entrar em liquidação.
    /// </summary>
    /// <param name="fundo">Fundo a ser validado.</param>
    /// <returns>True se pode entrar em liquidação, false caso contrário.</returns>
    public bool PodeLiquidar(Fundo fundo)
    {
        var podeEntrarEmLiquidacao = fundo.Situacao is SituacaoFundo.Ativo or SituacaoFundo.Suspenso;

        if (podeEntrarEmLiquidacao)
        {
            _logger.LogInformation(
                "Fundo {FundoId} pode entrar em liquidação - Situação: {Situacao}",
                fundo.Id, fundo.Situacao);
        }
        else
        {
            _logger.LogWarning(
                "Fundo {FundoId} não pode entrar em liquidação - Situação inválida: {Situacao} (permitido: Ativo ou Suspenso)",
                fundo.Id, fundo.Situacao);
        }

        return podeEntrarEmLiquidacao;
    }

    /// <summary>
    ///     Calcula o progresso do cadastro do fundo (0-100).
    /// </summary>
    /// <param name="fundo">Fundo a ser avaliado.</param>
    /// <param name="classes">Classes do fundo.</param>
    /// <param name="taxas">Taxas do fundo.</param>
    /// <param name="prazos">Prazos do fundo.</param>
    /// <param name="vinculos">Vínculos do fundo.</param>
    /// <returns>Percentual de progresso (0-100).</returns>
    public int CalcularProgressoCadastro(
        Fundo fundo,
        IEnumerable<FundoClasse> classes,
        IEnumerable<FundoTaxa> taxas,
        IEnumerable<FundoPrazo> prazos,
        IEnumerable<FundoVinculo> vinculos)
    {
        _logger.LogDebug("Calculando progresso do cadastro para fundo {FundoId}", fundo.Id);

        var progresso = 0;

        // Dados básicos (20%) - CNPJ, Razão Social, Tipo, Data Constituição
        if (!string.IsNullOrEmpty(fundo.Cnpj.Valor) &&
            !string.IsNullOrEmpty(fundo.RazaoSocial) &&
            fundo.DataConstituicao.HasValue)
        {
            progresso += ProgressoPesos.DadosBasicos;
        }

        // Classificação (15%) - Classificação CVM, Público Alvo, Tributação
        progresso += ProgressoPesos.Classificacao; // Sempre preenchido na criação

        // Parâmetros de cota (10%) - Classes configuradas
        var classesAtivas = classes.Where(c => c.DeletedAt == null).ToList();
        if (classesAtivas.Count != 0)
        {
            progresso += ProgressoPesos.ParametrosCota;
        }

        // Taxas configuradas (15%)
        var taxasAtivas = taxas.Where(t => t.Ativa && t.DataFimVigencia == null).ToList();
        if (taxasAtivas.Any(t => t.TipoTaxa == TipoTaxa.Administracao))
        {
            progresso += ProgressoPesos.Taxas;
        }

        // Prazos configurados (15%)
        var prazosAtivos = prazos.Where(p => p.Ativo).ToList();
        var temAplicacao = prazosAtivos.Any(p => p.TipoPrazo == TipoPrazoOperacional.Aplicacao);
        var temResgate = prazosAtivos.Any(p => p.TipoPrazo == TipoPrazoOperacional.Resgate);
        if (temAplicacao && temResgate)
        {
            progresso += ProgressoPesos.Prazos;
        }
        else if (temAplicacao || temResgate)
        {
            progresso += ProgressoPesos.Prazos / 2;
        }

        // Vínculos obrigatórios (20%)
        var vinculosVigentes = vinculos.Where(v => v.EstaVigente()).ToList();
        var vinculosEncontrados = VinculosObrigatorios
            .Count(tipo => vinculosVigentes.Any(v => v.TipoVinculo == tipo));

        var percentualVinculos = (vinculosEncontrados * ProgressoPesos.VinculosObrigatorios) /
                                 VinculosObrigatorios.Length;
        progresso += percentualVinculos;

        // Documentos (5%) - Reservado para implementação futura
        // progresso += ProgressoPesos.Documentos;

        var progressoFinal = Math.Min(progresso, 100);

        _logger.LogInformation(
            "Progresso do cadastro calculado para fundo {FundoId}: {Progresso}%",
            fundo.Id, progressoFinal);

        return progressoFinal;
    }

    /// <summary>
    ///     Valida os vínculos obrigatórios do fundo.
    /// </summary>
    /// <param name="vinculos">Vínculos do fundo.</param>
    /// <returns>Resultado da validação com vínculos faltantes.</returns>
    public FundoVinculoValidationResult ValidarVinculosObrigatorios(IEnumerable<FundoVinculo> vinculos)
    {
        _logger.LogDebug("Validando vínculos obrigatórios");

        var vinculosVigentes = vinculos.Where(v => v.EstaVigente()).ToList();
        var vinculosFaltantes = new List<TipoVinculoInstitucional>();

        foreach (var tipoObrigatorio in VinculosObrigatorios)
        {
            if (!vinculosVigentes.Any(v => v.TipoVinculo == tipoObrigatorio))
            {
                vinculosFaltantes.Add(tipoObrigatorio);
            }
        }

        var resultado = new FundoVinculoValidationResult
        {
            IsValid = vinculosFaltantes.Count == 0,
            VinculosFaltantes = vinculosFaltantes,
            Mensagem = vinculosFaltantes.Count == 0
                ? "Todos os vínculos obrigatórios estão configurados."
                : $"Vínculos obrigatórios faltantes: {string.Join(", ", vinculosFaltantes)}"
        };

        if (resultado.IsValid)
        {
            _logger.LogInformation(
                "Validação de vínculos obrigatórios passou - Todos os {Count} vínculos obrigatórios estão configurados",
                VinculosObrigatorios.Length);
        }
        else
        {
            _logger.LogWarning(
                "Validação de vínculos obrigatórios falhou - Faltam {Count} vínculos: {VinculosFaltantes}",
                vinculosFaltantes.Count,
                string.Join(", ", vinculosFaltantes));
        }

        return resultado;
    }

    /// <summary>
    ///     Valida se um tipo de taxa pode ser adicionado ao fundo.
    /// </summary>
    /// <param name="tipoTaxa">Tipo de taxa a ser adicionada.</param>
    /// <param name="taxasExistentes">Taxas existentes no fundo.</param>
    /// <param name="classeId">ID da classe (opcional).</param>
    /// <returns>True se pode adicionar, false se já existe taxa ativa do tipo.</returns>
    public bool PodeAdicionarTaxa(
        TipoTaxa tipoTaxa,
        IEnumerable<FundoTaxa> taxasExistentes,
        Guid? classeId = null)
    {
        var taxasAtivas = taxasExistentes
            .Where(t => t.Ativa && t.DataFimVigencia == null);

        if (classeId.HasValue)
        {
            return !taxasAtivas.Any(t => t.TipoTaxa == tipoTaxa && t.ClasseId == classeId);
        }

        return !taxasAtivas.Any(t => t.TipoTaxa == tipoTaxa && t.ClasseId == null);
    }
}

/// <summary>
///     Resultado da validação de vínculos obrigatórios.
/// </summary>
public class FundoVinculoValidationResult
{
    /// <summary>
    ///     Indica se a validação passou (todos os vínculos obrigatórios presentes).
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    ///     Lista de tipos de vínculo que estão faltando.
    /// </summary>
    public IReadOnlyList<TipoVinculoInstitucional> VinculosFaltantes { get; init; } =
        Array.Empty<TipoVinculoInstitucional>();

    /// <summary>
    ///     Mensagem descritiva do resultado.
    /// </summary>
    public string Mensagem { get; init; } = string.Empty;
}
