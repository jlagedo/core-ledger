namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Tipos de recebíveis aceitos por FIDCs.
/// </summary>
public enum TipoRecebiveis
{
    /// <summary>
    ///     Crédito Consignado.
    /// </summary>
    CreditoConsignado = 1,

    /// <summary>
    ///     Duplicata.
    /// </summary>
    Duplicata = 2,

    /// <summary>
    ///     Cédula de Crédito Bancário.
    /// </summary>
    CCB = 3,

    /// <summary>
    ///     Cheque.
    /// </summary>
    Cheque = 4,

    /// <summary>
    ///     Cartão de Crédito.
    /// </summary>
    CartaoCredito = 5,

    /// <summary>
    ///     Energia.
    /// </summary>
    Energia = 6,

    /// <summary>
    ///     Telecomunicações.
    /// </summary>
    Telecom = 7,

    /// <summary>
    ///     Precatórios.
    /// </summary>
    Precatorios = 8,

    /// <summary>
    ///     Outros tipos de recebíveis.
    /// </summary>
    Outros = 99
}
