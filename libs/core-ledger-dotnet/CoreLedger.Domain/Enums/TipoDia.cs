namespace CoreLedger.Domain.Enums;

/// <summary>
///     Representa o tipo de dia no calendário para cálculos de dias úteis.
/// </summary>
public enum TipoDia
{
    /// <summary>
    ///     Dia útil (dia útil normal).
    /// </summary>
    Util = 1,

    /// <summary>
    ///     Feriado nacional.
    /// </summary>
    FeriadoNacional = 2,

    /// <summary>
    ///     Feriado estadual.
    /// </summary>
    FeriadoEstadual = 3,

    /// <summary>
    ///     Feriado municipal.
    /// </summary>
    FeriadoMunicipal = 4,

    /// <summary>
    ///     Feriado bancário (p. ex., Véspera de Ano Novo).
    /// </summary>
    FeriadoBancario = 5,

    /// <summary>
    ///     Fim de semana (sábado ou domingo).
    /// </summary>
    FimDeSemana = 6,

    /// <summary>
    ///     Ponto facultativo.
    /// </summary>
    PontoFacultativo = 7
}
