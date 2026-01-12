using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Parâmetros de cálculo e exibição da cota do fundo.
///     Relacionamento 1:1 com Fundo.
/// </summary>
public class FundoParametrosCota
{
    /// <summary>
    ///     Identificador único (BIGINT SERIAL).
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Identificador do fundo (relacionamento 1:1).
    /// </summary>
    public Guid FundoId { get; private set; }

    /// <summary>
    ///     Número de casas decimais para a cota (padrão: 8, range: 4-10).
    /// </summary>
    public int CasasDecimaisCota { get; private set; }

    /// <summary>
    ///     Número de casas decimais para quantidade de cotas (padrão: 6, range: 4-8).
    /// </summary>
    public int CasasDecimaisQuantidade { get; private set; }

    /// <summary>
    ///     Número de casas decimais para o patrimônio líquido (padrão: 2, range: 2-4).
    /// </summary>
    public int CasasDecimaisPl { get; private set; }

    /// <summary>
    ///     Tipo de cálculo da cota (Fechamento ou Abertura).
    /// </summary>
    public TipoCota TipoCota { get; private set; }

    /// <summary>
    ///     Horário de corte para movimentações (HH:mm).
    /// </summary>
    public TimeOnly HorarioCorte { get; private set; }

    /// <summary>
    ///     Valor da cota na constituição do fundo.
    /// </summary>
    public decimal CotaInicial { get; private set; }

    /// <summary>
    ///     Data da primeira cota do fundo.
    /// </summary>
    public DateOnly DataCotaInicial { get; private set; }

    /// <summary>
    ///     Fuso horário para cálculos (padrão: America/Sao_Paulo).
    /// </summary>
    public string FusoHorario { get; private set; } = null!;

    /// <summary>
    ///     Indica se permite divulgação de cota estimada antes do fechamento oficial.
    /// </summary>
    public bool PermiteCotaEstimada { get; private set; }

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Data e hora da última atualização do registro.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    ///     Referência de navegação para o Fundo.
    /// </summary>
    public Fundo Fundo { get; private set; } = null!;

    // Construtor privado para EF Core
    private FundoParametrosCota()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de FundoParametrosCota com validações.
    /// </summary>
    public static FundoParametrosCota Criar(
        Guid fundoId,
        TipoCota tipoCota,
        TimeOnly horarioCorte,
        decimal cotaInicial,
        DateOnly dataCotaInicial,
        int casasDecimaisCota = 8,
        int casasDecimaisQuantidade = 6,
        int casasDecimaisPl = 2,
        string fusoHorario = "America/Sao_Paulo",
        bool permiteCotaEstimada = false)
    {
        ValidarCasasDecimais(casasDecimaisCota, 4, 10, "Casas decimais da cota");
        ValidarCasasDecimais(casasDecimaisQuantidade, 4, 8, "Casas decimais da quantidade");
        ValidarCasasDecimais(casasDecimaisPl, 2, 4, "Casas decimais do PL");

        if (cotaInicial <= 0)
            throw new DomainValidationException("Valor da cota inicial deve ser maior que zero.");

        if (cotaInicial > 1_000_000)
            throw new DomainValidationException("Valor da cota inicial deve ser no máximo R$ 1.000.000,00.");

        if (string.IsNullOrWhiteSpace(fusoHorario))
            throw new DomainValidationException("Fuso horário é obrigatório.");

        if (fusoHorario.Length > 50)
            throw new DomainValidationException("Fuso horário deve ter no máximo 50 caracteres.");

        return new FundoParametrosCota
        {
            FundoId = fundoId,
            TipoCota = tipoCota,
            HorarioCorte = horarioCorte,
            CotaInicial = cotaInicial,
            DataCotaInicial = dataCotaInicial,
            CasasDecimaisCota = casasDecimaisCota,
            CasasDecimaisQuantidade = casasDecimaisQuantidade,
            CasasDecimaisPl = casasDecimaisPl,
            FusoHorario = fusoHorario.Trim(),
            PermiteCotaEstimada = permiteCotaEstimada,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza os parâmetros de cota.
    /// </summary>
    public void Atualizar(
        TipoCota tipoCota,
        TimeOnly horarioCorte,
        decimal cotaInicial,
        DateOnly dataCotaInicial,
        int casasDecimaisCota,
        int casasDecimaisQuantidade,
        int casasDecimaisPl,
        string fusoHorario,
        bool permiteCotaEstimada)
    {
        ValidarCasasDecimais(casasDecimaisCota, 4, 10, "Casas decimais da cota");
        ValidarCasasDecimais(casasDecimaisQuantidade, 4, 8, "Casas decimais da quantidade");
        ValidarCasasDecimais(casasDecimaisPl, 2, 4, "Casas decimais do PL");

        if (cotaInicial <= 0)
            throw new DomainValidationException("Valor da cota inicial deve ser maior que zero.");

        if (cotaInicial > 1_000_000)
            throw new DomainValidationException("Valor da cota inicial deve ser no máximo R$ 1.000.000,00.");

        if (string.IsNullOrWhiteSpace(fusoHorario))
            throw new DomainValidationException("Fuso horário é obrigatório.");

        if (fusoHorario.Length > 50)
            throw new DomainValidationException("Fuso horário deve ter no máximo 50 caracteres.");

        TipoCota = tipoCota;
        HorarioCorte = horarioCorte;
        CotaInicial = cotaInicial;
        DataCotaInicial = dataCotaInicial;
        CasasDecimaisCota = casasDecimaisCota;
        CasasDecimaisQuantidade = casasDecimaisQuantidade;
        CasasDecimaisPl = casasDecimaisPl;
        FusoHorario = fusoHorario.Trim();
        PermiteCotaEstimada = permiteCotaEstimada;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Atualiza apenas o horário de corte.
    /// </summary>
    public void AtualizarHorarioCorte(TimeOnly horarioCorte)
    {
        HorarioCorte = horarioCorte;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Atualiza a permissão de cota estimada.
    /// </summary>
    public void AtualizarPermiteCotaEstimada(bool permiteCotaEstimada)
    {
        PermiteCotaEstimada = permiteCotaEstimada;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidarCasasDecimais(int valor, int minimo, int maximo, string nomeCampo)
    {
        if (valor < minimo || valor > maximo)
            throw new DomainValidationException(
                $"{nomeCampo} deve estar entre {minimo} e {maximo}.");
    }
}
