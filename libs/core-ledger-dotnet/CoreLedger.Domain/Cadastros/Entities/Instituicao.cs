using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Entidade representando uma instituição financeira (administrador, gestor, custodiante, etc.).
/// </summary>
public class Instituicao
{
    /// <summary>
    ///     Identificador único da instituição (SERIAL).
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     CNPJ da instituição (14 dígitos, sem formatação).
    /// </summary>
    public CNPJ Cnpj { get; private set; } = null!;

    /// <summary>
    ///     Razão social da instituição.
    /// </summary>
    public string RazaoSocial { get; private set; } = null!;

    /// <summary>
    ///     Nome fantasia da instituição.
    /// </summary>
    public string? NomeFantasia { get; private set; }

    /// <summary>
    ///     Indica se a instituição está ativa.
    /// </summary>
    public bool Ativo { get; private set; }

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Data e hora da última atualização do registro.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    ///     Vínculos desta instituição com fundos (relacionamento 1:N).
    /// </summary>
    public ICollection<FundoVinculo> Vinculos { get; private set; } = new List<FundoVinculo>();

    // Construtor privado para EF Core
    private Instituicao()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de Instituição com validações.
    /// </summary>
    public static Instituicao Criar(
        string cnpj,
        string razaoSocial,
        string? nomeFantasia = null,
        bool ativo = true)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new DomainValidationException("Razão social é obrigatória.");

        if (razaoSocial.Length > 200)
            throw new DomainValidationException("Razão social deve ter no máximo 200 caracteres.");

        if (nomeFantasia?.Length > 100)
            throw new DomainValidationException("Nome fantasia deve ter no máximo 100 caracteres.");

        var cnpjVO = CNPJ.Criar(cnpj);

        return new Instituicao
        {
            Cnpj = cnpjVO,
            RazaoSocial = razaoSocial.Trim(),
            NomeFantasia = nomeFantasia?.Trim(),
            Ativo = ativo,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza os dados cadastrais da instituição.
    /// </summary>
    public void AtualizarDadosCadastrais(
        string razaoSocial,
        string? nomeFantasia)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new DomainValidationException("Razão social é obrigatória.");

        if (razaoSocial.Length > 200)
            throw new DomainValidationException("Razão social deve ter no máximo 200 caracteres.");

        if (nomeFantasia?.Length > 100)
            throw new DomainValidationException("Nome fantasia deve ter no máximo 100 caracteres.");

        RazaoSocial = razaoSocial.Trim();
        NomeFantasia = nomeFantasia?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Ativa a instituição.
    /// </summary>
    public void Ativar()
    {
        Ativo = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Inativa a instituição.
    /// </summary>
    public void Inativar()
    {
        Ativo = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
