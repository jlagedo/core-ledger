using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
/// Representa os parâmetros específicos para taxa de performance.
/// </summary>
public class FundoTaxaPerformance
{
    /// <summary>
    /// Identificador único dos parâmetros de performance (BIGINT SERIAL).
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    /// Identificador da taxa à qual os parâmetros pertencem.
    /// </summary>
    public long FundoTaxaId { get; private set; }

    /// <summary>
    /// Taxa à qual os parâmetros pertencem.
    /// </summary>
    public FundoTaxa FundoTaxa { get; private set; } = null!;

    /// <summary>
    /// Identificador do indexador (benchmark).
    /// </summary>
    public int IndexadorId { get; private set; }

    /// <summary>
    /// Indexador usado como benchmark.
    /// </summary>
    public Indexador Indexador { get; private set; } = null!;

    /// <summary>
    /// Percentual do benchmark (ex: 100% CDI).
    /// </summary>
    public decimal PercentualBenchmark { get; private set; }

    /// <summary>
    /// Método de cálculo da taxa de performance.
    /// </summary>
    public MetodoCalculoPerformance MetodoCalculo { get; private set; }

    /// <summary>
    /// Indica se utiliza linha d'água (high water mark).
    /// </summary>
    public bool LinhaDagua { get; private set; }

    /// <summary>
    /// Periodicidade de cristalização da performance.
    /// </summary>
    public PeriodicidadeCristalizacao PeriodicidadeCristalizacao { get; private set; }

    /// <summary>
    /// Mês de cristalização (1-12).
    /// </summary>
    public int? MesCristalizacao { get; private set; }

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Construtor privado para EF Core.
    /// </summary>
    private FundoTaxaPerformance()
    {
    }

    /// <summary>
    /// Cria novos parâmetros de performance para uma taxa.
    /// </summary>
    /// <param name="fundoTaxaId">Identificador da taxa.</param>
    /// <param name="indexadorId">Identificador do indexador (benchmark).</param>
    /// <param name="percentualBenchmark">Percentual do benchmark.</param>
    /// <param name="metodoCalculo">Método de cálculo.</param>
    /// <param name="linhaDagua">Indica se utiliza linha d'água.</param>
    /// <param name="periodicidadeCristalizacao">Periodicidade de cristalização.</param>
    /// <param name="mesCristalizacao">Mês de cristalização (1-12).</param>
    /// <returns>Nova instância de FundoTaxaPerformance.</returns>
    /// <exception cref="DomainValidationException">Quando os dados são inválidos.</exception>
    public static FundoTaxaPerformance Criar(
        long fundoTaxaId,
        int indexadorId,
        decimal percentualBenchmark,
        MetodoCalculoPerformance metodoCalculo,
        bool linhaDagua,
        PeriodicidadeCristalizacao periodicidadeCristalizacao,
        int? mesCristalizacao = null)
    {
        ValidarFundoTaxaId(fundoTaxaId);
        ValidarIndexadorId(indexadorId);
        ValidarPercentualBenchmark(percentualBenchmark);
        ValidarMesCristalizacao(mesCristalizacao);

        return new FundoTaxaPerformance
        {
            FundoTaxaId = fundoTaxaId,
            IndexadorId = indexadorId,
            PercentualBenchmark = percentualBenchmark,
            MetodoCalculo = metodoCalculo,
            LinhaDagua = linhaDagua,
            PeriodicidadeCristalizacao = periodicidadeCristalizacao,
            MesCristalizacao = mesCristalizacao,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria parâmetros de performance sem ID da taxa (para criação junto com a taxa).
    /// </summary>
    /// <param name="indexadorId">Identificador do indexador (benchmark).</param>
    /// <param name="percentualBenchmark">Percentual do benchmark.</param>
    /// <param name="metodoCalculo">Método de cálculo.</param>
    /// <param name="linhaDagua">Indica se utiliza linha d'água.</param>
    /// <param name="periodicidadeCristalizacao">Periodicidade de cristalização.</param>
    /// <param name="mesCristalizacao">Mês de cristalização (1-12).</param>
    /// <returns>Nova instância de FundoTaxaPerformance.</returns>
    public static FundoTaxaPerformance CriarSemTaxa(
        int indexadorId,
        decimal percentualBenchmark,
        MetodoCalculoPerformance metodoCalculo,
        bool linhaDagua,
        PeriodicidadeCristalizacao periodicidadeCristalizacao,
        int? mesCristalizacao = null)
    {
        ValidarIndexadorId(indexadorId);
        ValidarPercentualBenchmark(percentualBenchmark);
        ValidarMesCristalizacao(mesCristalizacao);

        return new FundoTaxaPerformance
        {
            IndexadorId = indexadorId,
            PercentualBenchmark = percentualBenchmark,
            MetodoCalculo = metodoCalculo,
            LinhaDagua = linhaDagua,
            PeriodicidadeCristalizacao = periodicidadeCristalizacao,
            MesCristalizacao = mesCristalizacao,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Atualiza os parâmetros de performance.
    /// </summary>
    /// <param name="indexadorId">Novo identificador do indexador.</param>
    /// <param name="percentualBenchmark">Novo percentual do benchmark.</param>
    /// <param name="metodoCalculo">Novo método de cálculo.</param>
    /// <param name="linhaDagua">Indica se utiliza linha d'água.</param>
    /// <param name="periodicidadeCristalizacao">Nova periodicidade de cristalização.</param>
    /// <param name="mesCristalizacao">Novo mês de cristalização.</param>
    public void Atualizar(
        int indexadorId,
        decimal percentualBenchmark,
        MetodoCalculoPerformance metodoCalculo,
        bool linhaDagua,
        PeriodicidadeCristalizacao periodicidadeCristalizacao,
        int? mesCristalizacao)
    {
        ValidarIndexadorId(indexadorId);
        ValidarPercentualBenchmark(percentualBenchmark);
        ValidarMesCristalizacao(mesCristalizacao);

        IndexadorId = indexadorId;
        PercentualBenchmark = percentualBenchmark;
        MetodoCalculo = metodoCalculo;
        LinhaDagua = linhaDagua;
        PeriodicidadeCristalizacao = periodicidadeCristalizacao;
        MesCristalizacao = mesCristalizacao;
    }

    private static void ValidarFundoTaxaId(long fundoTaxaId)
    {
        if (fundoTaxaId <= 0)
        {
            throw new DomainValidationException("FundoTaxaId é obrigatório.");
        }
    }

    private static void ValidarIndexadorId(int indexadorId)
    {
        if (indexadorId <= 0)
        {
            throw new DomainValidationException("IndexadorId (benchmark) é obrigatório.");
        }
    }

    private static void ValidarPercentualBenchmark(decimal percentualBenchmark)
    {
        if (percentualBenchmark <= 0)
        {
            throw new DomainValidationException("Percentual do benchmark deve ser maior que zero.");
        }

        if (percentualBenchmark > 1000)
        {
            throw new DomainValidationException("Percentual do benchmark não pode ser maior que 1000%.");
        }
    }

    private static void ValidarMesCristalizacao(int? mesCristalizacao)
    {
        if (mesCristalizacao.HasValue && (mesCristalizacao.Value < 1 || mesCristalizacao.Value > 12))
        {
            throw new DomainValidationException("Mês de cristalização deve ser entre 1 e 12.");
        }
    }
}
