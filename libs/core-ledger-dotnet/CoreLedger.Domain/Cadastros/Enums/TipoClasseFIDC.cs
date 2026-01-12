namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Tipo de classe de cotas em FIDC (Fundo de Investimento em Direitos Creditórios).
/// </summary>
public enum TipoClasseFIDC
{
    /// <summary>
    ///     Classe sênior - prioridade no recebimento e menor risco.
    /// </summary>
    Senior = 1,

    /// <summary>
    ///     Classe mezanino - prioridade intermediária.
    /// </summary>
    Mezanino = 2,

    /// <summary>
    ///     Classe subordinada - última prioridade e maior risco.
    /// </summary>
    Subordinada = 3
}
