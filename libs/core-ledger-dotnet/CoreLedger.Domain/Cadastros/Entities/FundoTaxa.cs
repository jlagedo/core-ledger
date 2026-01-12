using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
/// Representa uma taxa cobrada pelo fundo de investimento.
/// </summary>
public class FundoTaxa
{
    /// <summary>
    /// Identificador único da taxa (BIGINT SERIAL).
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    /// Identificador do fundo ao qual a taxa pertence.
    /// </summary>
    public Guid FundoId { get; private set; }

    /// <summary>
    /// Fundo ao qual a taxa pertence.
    /// </summary>
    public Fundo Fundo { get; private set; } = null!;

    /// <summary>
    /// Identificador da classe (opcional - taxa específica por classe).
    /// </summary>
    public Guid? ClasseId { get; private set; }

    /// <summary>
    /// Classe à qual a taxa pertence (quando aplicável).
    /// </summary>
    public FundoClasse? Classe { get; private set; }

    /// <summary>
    /// Tipo da taxa.
    /// </summary>
    public TipoTaxa TipoTaxa { get; private set; }

    /// <summary>
    /// Percentual da taxa (% a.a.).
    /// </summary>
    public decimal Percentual { get; private set; }

    /// <summary>
    /// Base de cálculo da taxa.
    /// </summary>
    public BaseCalculoTaxa BaseCalculo { get; private set; }

    /// <summary>
    /// Periodicidade de provisão da taxa.
    /// </summary>
    public PeriodicidadeProvisao PeriodicidadeProvisao { get; private set; }

    /// <summary>
    /// Periodicidade de pagamento da taxa.
    /// </summary>
    public PeriodicidadePagamento PeriodicidadePagamento { get; private set; }

    /// <summary>
    /// Dia do mês para pagamento (1-28).
    /// </summary>
    public int? DiaPagamento { get; private set; }

    /// <summary>
    /// Valor mínimo mensal da taxa.
    /// </summary>
    public decimal? ValorMinimo { get; private set; }

    /// <summary>
    /// Valor máximo (cap) da taxa.
    /// </summary>
    public decimal? ValorMaximo { get; private set; }

    /// <summary>
    /// Data de início da vigência da taxa.
    /// </summary>
    public DateOnly DataInicioVigencia { get; private set; }

    /// <summary>
    /// Data de fim da vigência da taxa (null = vigente).
    /// </summary>
    public DateOnly? DataFimVigencia { get; private set; }

    /// <summary>
    /// Indica se a taxa está ativa.
    /// </summary>
    public bool Ativa { get; private set; }

    /// <summary>
    /// Parâmetros específicos para taxa de performance.
    /// </summary>
    public FundoTaxaPerformance? ParametrosPerformance { get; private set; }

    /// <summary>
    /// Indica se a linha d'água é global (true) ou por cotista (false).
    /// Aplicável apenas para taxas de performance com HWM.
    /// </summary>
    public bool? LinhaDaguaGlobal { get; private set; }

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Data da última atualização do registro.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Construtor privado para EF Core.
    /// </summary>
    private FundoTaxa()
    {
    }

    /// <summary>
    /// Cria uma nova taxa para o fundo.
    /// </summary>
    /// <param name="fundoId">Identificador do fundo.</param>
    /// <param name="tipoTaxa">Tipo da taxa.</param>
    /// <param name="percentual">Percentual da taxa (% a.a.).</param>
    /// <param name="baseCalculo">Base de cálculo.</param>
    /// <param name="periodicidadeProvisao">Periodicidade de provisão.</param>
    /// <param name="periodicidadePagamento">Periodicidade de pagamento.</param>
    /// <param name="dataInicioVigencia">Data de início da vigência.</param>
    /// <param name="classeId">Identificador da classe (opcional).</param>
    /// <param name="diaPagamento">Dia do mês para pagamento (1-28).</param>
    /// <param name="valorMinimo">Valor mínimo mensal.</param>
    /// <param name="valorMaximo">Valor máximo (cap).</param>
    /// <returns>Nova instância de FundoTaxa.</returns>
    /// <exception cref="DomainValidationException">Quando os dados são inválidos.</exception>
    public static FundoTaxa Criar(
        Guid fundoId,
        TipoTaxa tipoTaxa,
        decimal percentual,
        BaseCalculoTaxa baseCalculo,
        PeriodicidadeProvisao periodicidadeProvisao,
        PeriodicidadePagamento periodicidadePagamento,
        DateOnly dataInicioVigencia,
        Guid? classeId = null,
        int? diaPagamento = null,
        decimal? valorMinimo = null,
        decimal? valorMaximo = null)
    {
        ValidarFundoId(fundoId);
        ValidarPercentual(percentual);
        ValidarDiaPagamento(diaPagamento);
        ValidarValoresMinMax(valorMinimo, valorMaximo);

        return new FundoTaxa
        {
            FundoId = fundoId,
            ClasseId = classeId,
            TipoTaxa = tipoTaxa,
            Percentual = percentual,
            BaseCalculo = baseCalculo,
            PeriodicidadeProvisao = periodicidadeProvisao,
            PeriodicidadePagamento = periodicidadePagamento,
            DiaPagamento = diaPagamento,
            ValorMinimo = valorMinimo,
            ValorMaximo = valorMaximo,
            DataInicioVigencia = dataInicioVigencia,
            DataFimVigencia = null,
            Ativa = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Atualiza os dados da taxa.
    /// </summary>
    /// <param name="percentual">Novo percentual.</param>
    /// <param name="baseCalculo">Nova base de cálculo.</param>
    /// <param name="periodicidadeProvisao">Nova periodicidade de provisão.</param>
    /// <param name="periodicidadePagamento">Nova periodicidade de pagamento.</param>
    /// <param name="diaPagamento">Novo dia de pagamento.</param>
    /// <param name="valorMinimo">Novo valor mínimo.</param>
    /// <param name="valorMaximo">Novo valor máximo.</param>
    public void Atualizar(
        decimal percentual,
        BaseCalculoTaxa baseCalculo,
        PeriodicidadeProvisao periodicidadeProvisao,
        PeriodicidadePagamento periodicidadePagamento,
        int? diaPagamento,
        decimal? valorMinimo,
        decimal? valorMaximo)
    {
        ValidarPercentual(percentual);
        ValidarDiaPagamento(diaPagamento);
        ValidarValoresMinMax(valorMinimo, valorMaximo);

        Percentual = percentual;
        BaseCalculo = baseCalculo;
        PeriodicidadeProvisao = periodicidadeProvisao;
        PeriodicidadePagamento = periodicidadePagamento;
        DiaPagamento = diaPagamento;
        ValorMinimo = valorMinimo;
        ValorMaximo = valorMaximo;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desativa a taxa, definindo a data de fim de vigência.
    /// </summary>
    /// <param name="dataFim">Data de fim da vigência (default: hoje).</param>
    public void Desativar(DateOnly? dataFim = null)
    {
        var dataFimVigencia = dataFim ?? DateOnly.FromDateTime(DateTime.UtcNow);

        if (dataFimVigencia < DataInicioVigencia)
        {
            throw new DomainValidationException(
                "Data de fim de vigência não pode ser anterior à data de início.");
        }

        DataFimVigencia = dataFimVigencia;
        Ativa = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reativa a taxa.
    /// </summary>
    public void Reativar()
    {
        DataFimVigencia = null;
        Ativa = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Define os parâmetros de performance para a taxa.
    /// </summary>
    /// <param name="parametros">Parâmetros de performance.</param>
    /// <exception cref="DomainValidationException">Quando a taxa não é do tipo Performance.</exception>
    public void DefinirParametrosPerformance(FundoTaxaPerformance parametros)
    {
        if (TipoTaxa != TipoTaxa.Performance)
        {
            throw new DomainValidationException(
                "Parâmetros de performance só podem ser definidos para taxas do tipo Performance.");
        }

        ParametrosPerformance = parametros ?? throw new DomainValidationException(
            "Parâmetros de performance são obrigatórios para taxa do tipo Performance.");

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a taxa é do tipo Performance.
    /// </summary>
    public bool EhTaxaPerformance => TipoTaxa == TipoTaxa.Performance;

    /// <summary>
    /// Verifica se a taxa requer parâmetros de performance.
    /// </summary>
    public bool RequerParametrosPerformance => EhTaxaPerformance && ParametrosPerformance == null;

    private static void ValidarFundoId(Guid fundoId)
    {
        if (fundoId == Guid.Empty)
        {
            throw new DomainValidationException("FundoId é obrigatório.");
        }
    }

    private static void ValidarPercentual(decimal percentual)
    {
        if (percentual < 0)
        {
            throw new DomainValidationException("Percentual não pode ser negativo.");
        }

        if (percentual > 100)
        {
            throw new DomainValidationException("Percentual não pode ser maior que 100%.");
        }
    }

    private static void ValidarDiaPagamento(int? diaPagamento)
    {
        if (diaPagamento.HasValue && (diaPagamento.Value < 1 || diaPagamento.Value > 28))
        {
            throw new DomainValidationException("Dia de pagamento deve ser entre 1 e 28.");
        }
    }

    private static void ValidarValoresMinMax(decimal? valorMinimo, decimal? valorMaximo)
    {
        if (valorMinimo.HasValue && valorMinimo.Value < 0)
        {
            throw new DomainValidationException("Valor mínimo não pode ser negativo.");
        }

        if (valorMaximo.HasValue && valorMaximo.Value < 0)
        {
            throw new DomainValidationException("Valor máximo não pode ser negativo.");
        }

        if (valorMinimo.HasValue && valorMaximo.HasValue && valorMinimo.Value > valorMaximo.Value)
        {
            throw new DomainValidationException("Valor mínimo não pode ser maior que valor máximo.");
        }
    }
}
