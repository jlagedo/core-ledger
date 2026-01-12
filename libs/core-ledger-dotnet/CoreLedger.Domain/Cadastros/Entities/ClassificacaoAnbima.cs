using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Entidade representando uma classificação ANBIMA de fundos de investimento.
/// </summary>
public class ClassificacaoAnbima : BaseEntity
{
    /// <summary>
    ///     Código único da classificação (ex: "RF_DL_SOB").
    /// </summary>
    public string Codigo { get; private set; } = null!;

    /// <summary>
    ///     Nome completo da classificação (ex: "Renda Fixa Duração Baixa Soberano").
    /// </summary>
    public string Nome { get; private set; } = null!;

    /// <summary>
    ///     Nível 1 da hierarquia de classificação - Categoria principal (ex: "Renda Fixa", "Ações").
    /// </summary>
    public string Nivel1 { get; private set; } = null!;

    /// <summary>
    ///     Nível 2 da hierarquia de classificação - Tipo (ex: "Duração Baixa", "Indexados").
    /// </summary>
    public string Nivel2 { get; private set; } = null!;

    /// <summary>
    ///     Nível 3 da hierarquia de classificação - Subtipo (ex: "Soberano", "Crédito Livre"). Pode ser nulo.
    /// </summary>
    public string? Nivel3 { get; private set; }

    /// <summary>
    ///     Classificação CVM correspondente (ex: "RENDA_FIXA", "ACOES"). Foreign key lógica para classificações CVM.
    /// </summary>
    public string ClassificacaoCvm { get; private set; } = null!;

    /// <summary>
    ///     Descrição detalhada da classificação ANBIMA.
    /// </summary>
    public string? Descricao { get; private set; }

    /// <summary>
    ///     Indica se a classificação está ativa.
    /// </summary>
    public bool Ativo { get; private set; }

    /// <summary>
    ///     Ordem de exibição da classificação em listas.
    /// </summary>
    public int OrdemExibicao { get; private set; }

    // Construtor privado para EF Core
    private ClassificacaoAnbima()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de ClassificacaoAnbima com validações.
    /// </summary>
    public static ClassificacaoAnbima Criar(
        string codigo,
        string nome,
        string nivel1,
        string nivel2,
        string? nivel3,
        string classificacaoCvm,
        string? descricao = null,
        bool ativo = true,
        int ordemExibicao = 0)
    {
        ValidarCodigo(codigo);
        ValidarNome(nome);
        ValidarNivel1(nivel1);
        ValidarNivel2(nivel2);
        ValidarNivel3(nivel3);
        ValidarClassificacaoCvm(classificacaoCvm);
        ValidarDescricao(descricao);

        return new ClassificacaoAnbima
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Nome = nome.Trim(),
            Nivel1 = nivel1.Trim(),
            Nivel2 = nivel2.Trim(),
            Nivel3 = nivel3?.Trim(),
            ClassificacaoCvm = classificacaoCvm.Trim().ToUpperInvariant(),
            Descricao = descricao?.Trim(),
            Ativo = ativo,
            OrdemExibicao = ordemExibicao
        };
    }

    /// <summary>
    ///     Atualiza os dados da classificação ANBIMA.
    /// </summary>
    public void Atualizar(
        string nome,
        string nivel1,
        string nivel2,
        string? nivel3,
        string? descricao,
        int ordemExibicao)
    {
        ValidarNome(nome);
        ValidarNivel1(nivel1);
        ValidarNivel2(nivel2);
        ValidarNivel3(nivel3);
        ValidarDescricao(descricao);

        Nome = nome.Trim();
        Nivel1 = nivel1.Trim();
        Nivel2 = nivel2.Trim();
        Nivel3 = nivel3?.Trim();
        Descricao = descricao?.Trim();
        OrdemExibicao = ordemExibicao;

        SetUpdated();
    }

    /// <summary>
    ///     Ativa a classificação ANBIMA.
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        SetUpdated();
    }

    /// <summary>
    ///     Inativa a classificação ANBIMA.
    /// </summary>
    public void Inativar()
    {
        Ativo = false;
        SetUpdated();
    }

    #region Validações

    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainValidationException("Código da classificação ANBIMA é obrigatório.");

        if (codigo.Length > 20)
            throw new DomainValidationException("Código da classificação ANBIMA deve ter no máximo 20 caracteres.");
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainValidationException("Nome da classificação ANBIMA é obrigatório.");

        if (nome.Length > 100)
            throw new DomainValidationException("Nome da classificação ANBIMA deve ter no máximo 100 caracteres.");
    }

    private static void ValidarNivel1(string nivel1)
    {
        if (string.IsNullOrWhiteSpace(nivel1))
            throw new DomainValidationException("Nível 1 da classificação ANBIMA é obrigatório.");

        if (nivel1.Length > 50)
            throw new DomainValidationException("Nível 1 da classificação ANBIMA deve ter no máximo 50 caracteres.");
    }

    private static void ValidarNivel2(string nivel2)
    {
        if (string.IsNullOrWhiteSpace(nivel2))
            throw new DomainValidationException("Nível 2 da classificação ANBIMA é obrigatório.");

        if (nivel2.Length > 50)
            throw new DomainValidationException("Nível 2 da classificação ANBIMA deve ter no máximo 50 caracteres.");
    }

    private static void ValidarNivel3(string? nivel3)
    {
        if (nivel3?.Length > 50)
            throw new DomainValidationException("Nível 3 da classificação ANBIMA deve ter no máximo 50 caracteres.");
    }

    private static void ValidarClassificacaoCvm(string classificacaoCvm)
    {
        if (string.IsNullOrWhiteSpace(classificacaoCvm))
            throw new DomainValidationException("Classificação CVM é obrigatória.");

        if (classificacaoCvm.Length > 30)
            throw new DomainValidationException("Classificação CVM deve ter no máximo 30 caracteres.");
    }

    private static void ValidarDescricao(string? descricao)
    {
        // Descricao é opcional, validar apenas se fornecida
        if (descricao != null && descricao.Length > 1000)
            throw new DomainValidationException("Descrição da classificação ANBIMA deve ter no máximo 1000 caracteres.");
    }

    #endregion
}
