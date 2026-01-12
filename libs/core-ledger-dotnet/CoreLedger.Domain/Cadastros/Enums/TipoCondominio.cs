namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Tipo de condomínio do fundo de investimento.
/// </summary>
public enum TipoCondominio
{
    /// <summary>
    ///     Condomínio aberto - permite resgate de cotas.
    /// </summary>
    Aberto = 1,

    /// <summary>
    ///     Condomínio fechado - não permite resgate de cotas.
    /// </summary>
    Fechado = 2
}
