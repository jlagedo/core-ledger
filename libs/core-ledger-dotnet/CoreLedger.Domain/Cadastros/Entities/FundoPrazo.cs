using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Representa prazos operacionais de aplicação, resgate e carência de um Fundo de Investimento.
/// </summary>
public class FundoPrazo
{
    /// <summary>
    ///     Identificador único do prazo (BIGINT SERIAL).
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Identificador do fundo ao qual o prazo pertence.
    /// </summary>
    public Guid FundoId { get; private set; }

    /// <summary>
    ///     Referência de navegação para o Fundo.
    /// </summary>
    public Fundo Fundo { get; private set; } = null!;

    /// <summary>
    ///     Identificador da classe (opcional - prazo específico por classe).
    /// </summary>
    public Guid? ClasseId { get; private set; }

    /// <summary>
    ///     Referência de navegação para a Classe (quando aplicável).
    /// </summary>
    public FundoClasse? Classe { get; private set; }

    /// <summary>
    ///     Tipo do prazo operacional (Aplicação, Resgate, Carência).
    /// </summary>
    public TipoPrazoOperacional TipoPrazo { get; private set; }

    /// <summary>
    ///     Dias para cotização (D+X).
    /// </summary>
    public int DiasCotizacao { get; private set; }

    /// <summary>
    ///     Dias para liquidação financeira (D+X).
    /// </summary>
    public int DiasLiquidacao { get; private set; }

    /// <summary>
    ///     Dias de carência inicial (aplicável para carência ou resgate).
    /// </summary>
    public int? DiasCarencia { get; private set; }

    /// <summary>
    ///     Horário limite para solicitação (horário de corte).
    /// </summary>
    public TimeOnly HorarioLimite { get; private set; }

    /// <summary>
    ///     Indica se os dias são úteis (true) ou corridos (false).
    /// </summary>
    public bool DiasUteis { get; private set; }

    /// <summary>
    ///     Identificador do calendário específico (FK para módulo de calendário).
    /// </summary>
    public int? CalendarioId { get; private set; }

    /// <summary>
    ///     Indica se permite operação parcial (resgate parcial).
    /// </summary>
    public bool PermiteParcial { get; private set; }

    /// <summary>
    ///     Tipo de calendário para contagem de dias D+X.
    ///     Valores: NACIONAL, SAO_PAULO, RIO_DE_JANEIRO, EXTERIOR_EUA, EXTERIOR_EUR.
    /// </summary>
    public string TipoCalendario { get; private set; } = "NACIONAL";

    /// <summary>
    ///     Indica se permite resgate programado/agendado.
    /// </summary>
    public bool PermiteResgateProgramado { get; private set; }

    /// <summary>
    ///     Prazo máximo em dias úteis para programar resgate.
    /// </summary>
    public int? PrazoMaximoProgramacao { get; private set; }

    /// <summary>
    ///     Percentual mínimo para resgate parcial (%).
    /// </summary>
    public decimal? PercentualMinimo { get; private set; }

    /// <summary>
    ///     Valor mínimo para operação.
    /// </summary>
    public decimal? ValorMinimo { get; private set; }

    /// <summary>
    ///     Indica se o prazo está ativo.
    /// </summary>
    public bool Ativo { get; private set; }

    /// <summary>
    ///     Exceções de prazo para períodos especiais.
    /// </summary>
    public ICollection<FundoPrazoExcecao> Excecoes { get; private set; } = new List<FundoPrazoExcecao>();

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Data e hora da última atualização do registro.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    // Construtor privado para EF Core
    private FundoPrazo()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de FundoPrazo com validações.
    /// </summary>
    /// <param name="fundoId">Identificador do fundo.</param>
    /// <param name="tipoPrazo">Tipo do prazo operacional.</param>
    /// <param name="diasCotizacao">Dias para cotização (D+X).</param>
    /// <param name="diasLiquidacao">Dias para liquidação (D+X).</param>
    /// <param name="horarioLimite">Horário limite para solicitação.</param>
    /// <param name="diasUteis">Se os dias são úteis ou corridos.</param>
    /// <param name="classeId">Identificador da classe (opcional).</param>
    /// <param name="diasCarencia">Dias de carência inicial.</param>
    /// <param name="calendarioId">Identificador do calendário específico.</param>
    /// <param name="permiteParcial">Se permite operação parcial.</param>
    /// <param name="percentualMinimo">Percentual mínimo para operação parcial.</param>
    /// <param name="valorMinimo">Valor mínimo para operação.</param>
    /// <param name="tipoCalendario">Tipo de calendário para D+X.</param>
    /// <param name="permiteResgateProgramado">Se permite resgate programado.</param>
    /// <param name="prazoMaximoProgramacao">Prazo máximo para programar resgate.</param>
    /// <returns>Nova instância de FundoPrazo.</returns>
    /// <exception cref="DomainValidationException">Quando os dados são inválidos.</exception>
    public static FundoPrazo Criar(
        Guid fundoId,
        TipoPrazoOperacional tipoPrazo,
        int diasCotizacao,
        int diasLiquidacao,
        TimeOnly horarioLimite,
        bool diasUteis,
        Guid? classeId = null,
        int? diasCarencia = null,
        int? calendarioId = null,
        bool permiteParcial = false,
        decimal? percentualMinimo = null,
        decimal? valorMinimo = null,
        string tipoCalendario = "NACIONAL",
        bool permiteResgateProgramado = false,
        int? prazoMaximoProgramacao = null)
    {
        ValidarFundoId(fundoId);
        ValidarDias(diasCotizacao, diasLiquidacao, diasCarencia);
        ValidarPercentualMinimo(percentualMinimo);
        ValidarValorMinimo(valorMinimo);
        ValidarTipoCalendario(tipoCalendario);
        ValidarPrazoMaximoProgramacao(permiteResgateProgramado, prazoMaximoProgramacao);

        return new FundoPrazo
        {
            FundoId = fundoId,
            ClasseId = classeId,
            TipoPrazo = tipoPrazo,
            DiasCotizacao = diasCotizacao,
            DiasLiquidacao = diasLiquidacao,
            DiasCarencia = diasCarencia,
            HorarioLimite = horarioLimite,
            DiasUteis = diasUteis,
            CalendarioId = calendarioId,
            PermiteParcial = permiteParcial,
            PercentualMinimo = percentualMinimo,
            ValorMinimo = valorMinimo,
            TipoCalendario = tipoCalendario.Trim().ToUpperInvariant(),
            PermiteResgateProgramado = permiteResgateProgramado,
            PrazoMaximoProgramacao = prazoMaximoProgramacao,
            Ativo = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza os dados do prazo operacional.
    /// </summary>
    /// <param name="diasCotizacao">Dias para cotização.</param>
    /// <param name="diasLiquidacao">Dias para liquidação.</param>
    /// <param name="horarioLimite">Horário limite.</param>
    /// <param name="diasUteis">Se os dias são úteis.</param>
    /// <param name="diasCarencia">Dias de carência.</param>
    /// <param name="calendarioId">Identificador do calendário.</param>
    /// <param name="permiteParcial">Se permite operação parcial.</param>
    /// <param name="percentualMinimo">Percentual mínimo.</param>
    /// <param name="valorMinimo">Valor mínimo.</param>
    /// <param name="tipoCalendario">Tipo de calendário para D+X.</param>
    /// <param name="permiteResgateProgramado">Se permite resgate programado.</param>
    /// <param name="prazoMaximoProgramacao">Prazo máximo para programar resgate.</param>
    public void Atualizar(
        int diasCotizacao,
        int diasLiquidacao,
        TimeOnly horarioLimite,
        bool diasUteis,
        int? diasCarencia,
        int? calendarioId,
        bool permiteParcial,
        decimal? percentualMinimo,
        decimal? valorMinimo,
        string tipoCalendario = "NACIONAL",
        bool permiteResgateProgramado = false,
        int? prazoMaximoProgramacao = null)
    {
        ValidarDias(diasCotizacao, diasLiquidacao, diasCarencia);
        ValidarPercentualMinimo(percentualMinimo);
        ValidarValorMinimo(valorMinimo);
        ValidarTipoCalendario(tipoCalendario);
        ValidarPrazoMaximoProgramacao(permiteResgateProgramado, prazoMaximoProgramacao);

        DiasCotizacao = diasCotizacao;
        DiasLiquidacao = diasLiquidacao;
        HorarioLimite = horarioLimite;
        DiasUteis = diasUteis;
        DiasCarencia = diasCarencia;
        CalendarioId = calendarioId;
        PermiteParcial = permiteParcial;
        PercentualMinimo = percentualMinimo;
        ValorMinimo = valorMinimo;
        TipoCalendario = tipoCalendario.Trim().ToUpperInvariant();
        PermiteResgateProgramado = permiteResgateProgramado;
        PrazoMaximoProgramacao = prazoMaximoProgramacao;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Ativa o prazo operacional.
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Desativa o prazo operacional.
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Adiciona uma exceção de prazo para período especial.
    /// </summary>
    /// <param name="excecao">Exceção de prazo a ser adicionada.</param>
    /// <exception cref="DomainValidationException">Quando há sobreposição de períodos.</exception>
    public void AdicionarExcecao(FundoPrazoExcecao excecao)
    {
        if (excecao == null)
            throw new DomainValidationException("Exceção não pode ser nula.");

        // Verifica sobreposição de períodos
        var temSobreposicao = Excecoes.Any(e =>
            (excecao.DataInicio >= e.DataInicio && excecao.DataInicio <= e.DataFim) ||
            (excecao.DataFim >= e.DataInicio && excecao.DataFim <= e.DataFim) ||
            (excecao.DataInicio <= e.DataInicio && excecao.DataFim >= e.DataFim));

        if (temSobreposicao)
            throw new DomainValidationException(
                "O período da exceção se sobrepõe a uma exceção existente.");

        Excecoes.Add(excecao);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Remove uma exceção de prazo.
    /// </summary>
    /// <param name="excecaoId">Identificador da exceção a ser removida.</param>
    public void RemoverExcecao(long excecaoId)
    {
        var excecao = Excecoes.FirstOrDefault(e => e.Id == excecaoId);
        if (excecao != null)
        {
            Excecoes.Remove(excecao);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    private static void ValidarFundoId(Guid fundoId)
    {
        if (fundoId == Guid.Empty)
            throw new DomainValidationException("FundoId é obrigatório.");
    }

    private static void ValidarDias(int diasCotizacao, int diasLiquidacao, int? diasCarencia)
    {
        if (diasCotizacao < 0)
            throw new DomainValidationException("Dias de cotização não pode ser negativo.");

        if (diasLiquidacao < 0)
            throw new DomainValidationException("Dias de liquidação não pode ser negativo.");

        if (diasCarencia.HasValue && diasCarencia.Value < 0)
            throw new DomainValidationException("Dias de carência não pode ser negativo.");
    }

    private static void ValidarPercentualMinimo(decimal? percentualMinimo)
    {
        if (percentualMinimo.HasValue && (percentualMinimo.Value < 0 || percentualMinimo.Value > 100))
            throw new DomainValidationException("Percentual mínimo deve estar entre 0 e 100.");
    }

    private static void ValidarValorMinimo(decimal? valorMinimo)
    {
        if (valorMinimo.HasValue && valorMinimo.Value < 0)
            throw new DomainValidationException("Valor mínimo não pode ser negativo.");
    }

    private static void ValidarTipoCalendario(string tipoCalendario)
    {
        var tiposValidos = new[] { "NACIONAL", "SAO_PAULO", "RIO_DE_JANEIRO", "EXTERIOR_EUA", "EXTERIOR_EUR" };
        if (!string.IsNullOrWhiteSpace(tipoCalendario) && !tiposValidos.Contains(tipoCalendario.Trim().ToUpperInvariant()))
            throw new DomainValidationException(
                $"Tipo de calendário inválido. Valores válidos: {string.Join(", ", tiposValidos)}");
    }

    private static void ValidarPrazoMaximoProgramacao(bool permiteResgateProgramado, int? prazoMaximoProgramacao)
    {
        if (permiteResgateProgramado && prazoMaximoProgramacao.HasValue && prazoMaximoProgramacao.Value <= 0)
            throw new DomainValidationException("Prazo máximo de programação deve ser maior que zero.");

        if (!permiteResgateProgramado && prazoMaximoProgramacao.HasValue)
            throw new DomainValidationException(
                "Prazo máximo de programação só pode ser informado se resgate programado for permitido.");
    }
}
