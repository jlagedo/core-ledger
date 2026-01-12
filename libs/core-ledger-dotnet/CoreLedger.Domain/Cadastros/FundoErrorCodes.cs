namespace CoreLedger.Domain.Cadastros;

/// <summary>
///     Códigos de erro padronizados para o módulo de Fundos.
/// </summary>
public static class FundoErrorCodes
{
    /// <summary>
    ///     Fundo não encontrado.
    /// </summary>
    public const string FundoNotFound = "FUNDO_NOT_FOUND";

    /// <summary>
    ///     CNPJ já cadastrado em outro fundo.
    /// </summary>
    public const string FundoCnpjExists = "FUNDO_CNPJ_EXISTS";

    /// <summary>
    ///     Fundo não pode ser excluído (possui movimentações).
    /// </summary>
    public const string FundoCannotDelete = "FUNDO_CANNOT_DELETE";

    /// <summary>
    ///     Classe não encontrada.
    /// </summary>
    public const string ClasseNotFound = "CLASSE_NOT_FOUND";

    /// <summary>
    ///     Código da classe já existe no fundo.
    /// </summary>
    public const string ClasseCodigoExists = "CLASSE_CODIGO_EXISTS";

    /// <summary>
    ///     Taxa do mesmo tipo já existe ativa.
    /// </summary>
    public const string TaxaDuplicada = "TAXA_DUPLICADA";

    /// <summary>
    ///     Taxa de performance requer benchmark (indexador).
    /// </summary>
    public const string TaxaBenchmarkRequired = "TAXA_BENCHMARK_REQUIRED";

    /// <summary>
    ///     Vínculo obrigatório faltando.
    /// </summary>
    public const string VinculoObrigatorio = "VINCULO_OBRIGATORIO";

    /// <summary>
    ///     Já existe vínculo principal do mesmo tipo.
    /// </summary>
    public const string VinculoPrincipalExists = "VINCULO_PRINCIPAL_EXISTS";

    /// <summary>
    ///     Instituição não encontrada.
    /// </summary>
    public const string InstituicaoNotFound = "INSTITUICAO_NOT_FOUND";

    /// <summary>
    ///     Calendário não encontrado.
    /// </summary>
    public const string CalendarioNotFound = "CALENDARIO_NOT_FOUND";

    /// <summary>
    ///     Prazo do mesmo tipo já existe para o fundo/classe.
    /// </summary>
    public const string PrazoDuplicado = "PRAZO_DUPLICADO";
}
