namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Tipo de vínculo institucional com o fundo de investimento.
/// </summary>
public enum TipoVinculoInstitucional
{
    /// <summary>
    ///     Administrador do fundo.
    /// </summary>
    Administrador = 1,

    /// <summary>
    ///     Gestor do fundo.
    /// </summary>
    Gestor = 2,

    /// <summary>
    ///     Custodiante do fundo.
    /// </summary>
    Custodiante = 3,

    /// <summary>
    ///     Distribuidor do fundo.
    /// </summary>
    Distribuidor = 4,

    /// <summary>
    ///     Auditor independente do fundo.
    /// </summary>
    Auditor = 5,

    /// <summary>
    ///     Escriturador de cotas.
    /// </summary>
    Escriturador = 6,

    /// <summary>
    ///     Controlador (se terceirizado).
    /// </summary>
    Controlador = 7,

    /// <summary>
    ///     Consultoria de crédito (FIDC).
    /// </summary>
    ConsultoriaCredito = 8,

    /// <summary>
    ///     Agente de cobrança (FIDC).
    /// </summary>
    AgenteCobranca = 9,

    /// <summary>
    ///     Cedente de recebíveis (FIDC).
    /// </summary>
    Cedente = 10,

    /// <summary>
    ///     Formador de mercado (ETF/FII).
    /// </summary>
    FormadorMercado = 11
}
