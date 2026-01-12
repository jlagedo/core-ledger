using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade de domínio HistóricoIndexador representando dados de série temporal para indexadores.
///     Dados históricos são imutáveis após a criação.
/// </summary>
public class HistoricoIndexador : BaseEntity
{
    private HistoricoIndexador()
    {
    }

    /// <summary>
    ///     Sobrescreve o Id base para usar BIGINT para grandes conjuntos de dados de série temporal.
    /// </summary>
    public new long Id { get; private set; }

    public int IndexadorId { get; private set; }
    public Indexador? Indexador { get; private set; }
    public DateTime DataReferencia { get; private set; }
    public decimal Valor { get; private set; }
    public decimal? FatorDiario { get; private set; }
    public decimal? VariacaoPercentual { get; private set; }
    public string? Fonte { get; private set; }
    public Guid? ImportacaoId { get; private set; }

    /// <summary>
    ///     Método factory para criar um novo HistóricoIndexador com validação.
    /// </summary>
    public static HistoricoIndexador Create(
        int indexadorId,
        DateTime dataReferencia,
        decimal valor,
        decimal? fatorDiario,
        decimal? variacaoPercentual,
        string? fonte,
        Guid? importacaoId)
    {
        ValidateIndexadorId(indexadorId);
        ValidateValor(valor);
        ValidateFatorDiario(fatorDiario);

        return new HistoricoIndexador
        {
            IndexadorId = indexadorId,
            DataReferencia = dataReferencia.Date,
            Valor = valor,
            FatorDiario = fatorDiario,
            VariacaoPercentual = variacaoPercentual,
            Fonte = fonte?.Trim(),
            ImportacaoId = importacaoId
        };
    }

    // NÃO há método Update - dados históricos são imutáveis

    private static void ValidateIndexadorId(int indexadorId)
    {
        if (indexadorId <= 0)
            throw new DomainValidationException("IndexadorId deve ser um número positivo");
    }

    private static void ValidateValor(decimal valor)
    {
        if (valor < 0)
            throw new DomainValidationException("Valor deve ser maior ou igual a zero");
    }

    private static void ValidateFatorDiario(decimal? fatorDiario)
    {
        if (fatorDiario.HasValue && fatorDiario.Value <= 0)
            throw new DomainValidationException("Fator diário deve ser maior que zero");
    }
}
