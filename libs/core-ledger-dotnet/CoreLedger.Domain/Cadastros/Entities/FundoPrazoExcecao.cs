using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Representa uma exceção temporária de prazo operacional para períodos especiais
///     (fechamento de balanço, eventos extraordinários, etc.).
/// </summary>
public class FundoPrazoExcecao
{
    /// <summary>
    ///     Identificador único da exceção (BIGINT SERIAL).
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Identificador do prazo ao qual a exceção pertence.
    /// </summary>
    public long PrazoId { get; private set; }

    /// <summary>
    ///     Referência de navegação para o FundoPrazo.
    /// </summary>
    public FundoPrazo Prazo { get; private set; } = null!;

    /// <summary>
    ///     Data de início da exceção.
    /// </summary>
    public DateOnly DataInicio { get; private set; }

    /// <summary>
    ///     Data de fim da exceção.
    /// </summary>
    public DateOnly DataFim { get; private set; }

    /// <summary>
    ///     Dias de cotização durante o período de exceção (override D+X).
    /// </summary>
    public int DiasCotizacao { get; private set; }

    /// <summary>
    ///     Dias de liquidação durante o período de exceção (override D+X).
    /// </summary>
    public int DiasLiquidacao { get; private set; }

    /// <summary>
    ///     Motivo da exceção (fechamento de balanço, feriado prolongado, etc.).
    /// </summary>
    public string Motivo { get; private set; } = null!;

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // Construtor privado para EF Core
    private FundoPrazoExcecao()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de FundoPrazoExcecao com validações.
    /// </summary>
    /// <param name="prazoId">Identificador do prazo.</param>
    /// <param name="dataInicio">Data de início da exceção.</param>
    /// <param name="dataFim">Data de fim da exceção.</param>
    /// <param name="diasCotizacao">Dias de cotização durante a exceção.</param>
    /// <param name="diasLiquidacao">Dias de liquidação durante a exceção.</param>
    /// <param name="motivo">Motivo da exceção.</param>
    /// <returns>Nova instância de FundoPrazoExcecao.</returns>
    /// <exception cref="DomainValidationException">Quando os dados são inválidos.</exception>
    public static FundoPrazoExcecao Criar(
        long prazoId,
        DateOnly dataInicio,
        DateOnly dataFim,
        int diasCotizacao,
        int diasLiquidacao,
        string motivo)
    {
        ValidarPrazoId(prazoId);
        ValidarPeriodo(dataInicio, dataFim);
        ValidarDias(diasCotizacao, diasLiquidacao);
        ValidarMotivo(motivo);

        return new FundoPrazoExcecao
        {
            PrazoId = prazoId,
            DataInicio = dataInicio,
            DataFim = dataFim,
            DiasCotizacao = diasCotizacao,
            DiasLiquidacao = diasLiquidacao,
            Motivo = motivo.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza os dados da exceção.
    /// </summary>
    /// <param name="dataInicio">Nova data de início.</param>
    /// <param name="dataFim">Nova data de fim.</param>
    /// <param name="diasCotizacao">Novos dias de cotização.</param>
    /// <param name="diasLiquidacao">Novos dias de liquidação.</param>
    /// <param name="motivo">Novo motivo.</param>
    public void Atualizar(
        DateOnly dataInicio,
        DateOnly dataFim,
        int diasCotizacao,
        int diasLiquidacao,
        string motivo)
    {
        ValidarPeriodo(dataInicio, dataFim);
        ValidarDias(diasCotizacao, diasLiquidacao);
        ValidarMotivo(motivo);

        DataInicio = dataInicio;
        DataFim = dataFim;
        DiasCotizacao = diasCotizacao;
        DiasLiquidacao = diasLiquidacao;
        Motivo = motivo.Trim();
    }

    /// <summary>
    ///     Verifica se a exceção está ativa em uma determinada data.
    /// </summary>
    /// <param name="data">Data a verificar.</param>
    /// <returns>True se a exceção está ativa na data.</returns>
    public bool EstaAtivaEm(DateOnly data)
    {
        return data >= DataInicio && data <= DataFim;
    }

    private static void ValidarPrazoId(long prazoId)
    {
        if (prazoId <= 0)
            throw new DomainValidationException("PrazoId é obrigatório.");
    }

    private static void ValidarPeriodo(DateOnly dataInicio, DateOnly dataFim)
    {
        if (dataFim < dataInicio)
            throw new DomainValidationException(
                "Data de fim não pode ser anterior à data de início.");
    }

    private static void ValidarDias(int diasCotizacao, int diasLiquidacao)
    {
        if (diasCotizacao < 0)
            throw new DomainValidationException("Dias de cotização não pode ser negativo.");

        if (diasLiquidacao < 0)
            throw new DomainValidationException("Dias de liquidação não pode ser negativo.");
    }

    private static void ValidarMotivo(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainValidationException("Motivo é obrigatório.");

        if (motivo.Length > 200)
            throw new DomainValidationException("Motivo deve ter no máximo 200 caracteres.");
    }
}
