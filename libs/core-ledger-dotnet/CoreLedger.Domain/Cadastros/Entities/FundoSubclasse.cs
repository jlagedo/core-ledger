using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Representa uma Subclasse de cotas de uma Classe de Fundo de Investimento.
///     Subclasses herdam parâmetros da classe pai se não especificados.
/// </summary>
public class FundoSubclasse
{
    /// <summary>
    ///     Identificador único da subclasse (BIGINT).
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Identificador da classe à qual a subclasse pertence.
    /// </summary>
    public Guid ClasseId { get; private set; }

    /// <summary>
    ///     Código identificador da subclasse.
    /// </summary>
    public string CodigoSubclasse { get; private set; } = null!;

    /// <summary>
    ///     Nome descritivo da subclasse.
    /// </summary>
    public string NomeSubclasse { get; private set; } = null!;

    /// <summary>
    ///     Número da série (se aplicável).
    /// </summary>
    public int? Serie { get; private set; }

    /// <summary>
    ///     Valor mínimo de aplicação na subclasse.
    ///     Se não especificado, herda da classe pai.
    /// </summary>
    public decimal? ValorMinimoAplicacao { get; private set; }

    /// <summary>
    ///     Taxa de administração diferenciada para esta subclasse (% a.a.).
    ///     Se não especificada, usa a taxa padrão do fundo.
    /// </summary>
    public decimal? TaxaAdministracaoDiferenciada { get; private set; }

    /// <summary>
    ///     Indica se a subclasse está ativa.
    /// </summary>
    public bool Ativa { get; private set; }

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Data e hora de exclusão lógica (soft delete).
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    ///     Referência de navegação para a Classe.
    /// </summary>
    public FundoClasse Classe { get; private set; } = null!;

    // Construtor privado para EF Core
    private FundoSubclasse()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de FundoSubclasse com validações.
    /// </summary>
    /// <param name="classeId">ID da classe pai</param>
    /// <param name="codigoSubclasse">Código da subclasse</param>
    /// <param name="nomeSubclasse">Nome da subclasse</param>
    /// <param name="serie">Número da série (opcional)</param>
    /// <param name="valorMinimoAplicacao">Valor mínimo de aplicação (opcional, herda da classe)</param>
    /// <param name="taxaAdministracaoDiferenciada">Taxa de administração diferenciada (opcional)</param>
    public static FundoSubclasse Criar(
        Guid classeId,
        string codigoSubclasse,
        string nomeSubclasse,
        int? serie = null,
        decimal? valorMinimoAplicacao = null,
        decimal? taxaAdministracaoDiferenciada = null)
    {
        ValidarParametros(codigoSubclasse, nomeSubclasse, serie, valorMinimoAplicacao, taxaAdministracaoDiferenciada);

        return new FundoSubclasse
        {
            ClasseId = classeId,
            CodigoSubclasse = codigoSubclasse.Trim().ToUpperInvariant(),
            NomeSubclasse = nomeSubclasse.Trim(),
            Serie = serie,
            ValorMinimoAplicacao = valorMinimoAplicacao,
            TaxaAdministracaoDiferenciada = taxaAdministracaoDiferenciada,
            Ativa = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza os dados da subclasse.
    /// </summary>
    public void Atualizar(
        string nomeSubclasse,
        int? serie,
        decimal? valorMinimoAplicacao,
        decimal? taxaAdministracaoDiferenciada)
    {
        ValidarParametros(CodigoSubclasse, nomeSubclasse, serie, valorMinimoAplicacao, taxaAdministracaoDiferenciada);

        NomeSubclasse = nomeSubclasse.Trim();
        Serie = serie;
        ValorMinimoAplicacao = valorMinimoAplicacao;
        TaxaAdministracaoDiferenciada = taxaAdministracaoDiferenciada;
    }

    /// <summary>
    ///     Ativa a subclasse.
    /// </summary>
    public void Ativar()
    {
        Ativa = true;
    }

    /// <summary>
    ///     Desativa a subclasse.
    /// </summary>
    public void Desativar()
    {
        Ativa = false;
    }

    /// <summary>
    ///     Marca a subclasse como excluída (soft delete).
    /// </summary>
    public void Excluir()
    {
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Restaura uma subclasse excluída.
    /// </summary>
    public void Restaurar()
    {
        DeletedAt = null;
    }

    /// <summary>
    ///     Obtém o valor mínimo de aplicação efetivo.
    ///     Retorna o valor próprio ou herda da classe pai.
    /// </summary>
    public decimal? ObterValorMinimoAplicacaoEfetivo()
    {
        return ValorMinimoAplicacao ?? Classe?.ValorMinimoAplicacao;
    }

    private static void ValidarParametros(
        string codigoSubclasse,
        string nomeSubclasse,
        int? serie,
        decimal? valorMinimoAplicacao,
        decimal? taxaAdministracaoDiferenciada)
    {
        if (string.IsNullOrWhiteSpace(codigoSubclasse))
            throw new DomainValidationException("Código da subclasse é obrigatório.");

        if (codigoSubclasse.Length > 10)
            throw new DomainValidationException("Código da subclasse deve ter no máximo 10 caracteres.");

        if (string.IsNullOrWhiteSpace(nomeSubclasse))
            throw new DomainValidationException("Nome da subclasse é obrigatório.");

        if (nomeSubclasse.Length > 100)
            throw new DomainValidationException("Nome da subclasse deve ter no máximo 100 caracteres.");

        if (serie.HasValue && serie < 1)
            throw new DomainValidationException("Série deve ser maior que zero.");

        if (valorMinimoAplicacao.HasValue && valorMinimoAplicacao < 0)
            throw new DomainValidationException("Valor mínimo de aplicação não pode ser negativo.");

        if (taxaAdministracaoDiferenciada.HasValue && taxaAdministracaoDiferenciada < 0)
            throw new DomainValidationException("Taxa de administração diferenciada não pode ser negativa.");
    }
}
