using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Representa uma Classe de cotas de um Fundo de Investimento conforme CVM 175.
///     Cada fundo pode ter múltiplas classes com características distintas.
/// </summary>
public class FundoClasse
{
    /// <summary>
    ///     Identificador único da classe (UUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Identificador do fundo ao qual a classe pertence.
    /// </summary>
    public Guid FundoId { get; private set; }

    /// <summary>
    ///     CNPJ próprio da classe (se aplicável).
    /// </summary>
    public string? CnpjClasse { get; private set; }

    /// <summary>
    ///     Código identificador da classe (Ex: SR, MEZ, SUB).
    /// </summary>
    public string CodigoClasse { get; private set; } = null!;

    /// <summary>
    ///     Nome descritivo da classe.
    /// </summary>
    public string NomeClasse { get; private set; } = null!;

    /// <summary>
    ///     Tipo da classe para FIDCs (Sênior, Mezanino, Subordinada).
    /// </summary>
    public TipoClasseFIDC? TipoClasseFidc { get; private set; }

    /// <summary>
    ///     Ordem de subordinação para FIDCs (prioridade no recebimento).
    /// </summary>
    public int? OrdemSubordinacao { get; private set; }

    /// <summary>
    ///     Rentabilidade alvo da classe (% a.a.).
    /// </summary>
    public decimal? RentabilidadeAlvo { get; private set; }

    /// <summary>
    ///     Indica se a classe possui responsabilidade limitada.
    /// </summary>
    public bool ResponsabilidadeLimitada { get; private set; }

    /// <summary>
    ///     Indica se a classe possui segregação patrimonial.
    /// </summary>
    public bool SegregacaoPatrimonial { get; private set; }

    /// <summary>
    ///     Valor mínimo de aplicação na classe.
    /// </summary>
    public decimal? ValorMinimoAplicacao { get; private set; }

    /// <summary>
    ///     Indica se a classe está ativa.
    /// </summary>
    public bool Ativa { get; private set; }

    /// <summary>
    ///     Indica se a classe permite resgate antecipado.
    /// </summary>
    public bool PermiteResgateAntecipado { get; private set; } = true;

    /// <summary>
    ///     Data de encerramento da classe (se encerrada).
    /// </summary>
    public DateOnly? DataEncerramento { get; private set; }

    /// <summary>
    ///     Motivo do encerramento da classe.
    /// </summary>
    public string? MotivoEncerramento { get; private set; }

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Data e hora da última atualização do registro.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    ///     Data e hora de exclusão lógica (soft delete).
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    ///     Referência de navegação para o Fundo.
    /// </summary>
    public Fundo Fundo { get; private set; } = null!;

    /// <summary>
    ///     Subclasses associadas a esta classe.
    /// </summary>
    public ICollection<FundoSubclasse> Subclasses { get; private set; } = new List<FundoSubclasse>();

    /// <summary>
    ///     Taxas específicas desta classe.
    /// </summary>
    public ICollection<FundoTaxa> Taxas { get; private set; } = new List<FundoTaxa>();

    // Construtor privado para EF Core
    private FundoClasse()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de FundoClasse com validações.
    /// </summary>
    /// <param name="fundoId">ID do fundo</param>
    /// <param name="codigoClasse">Código da classe</param>
    /// <param name="nomeClasse">Nome da classe</param>
    /// <param name="tipoFundo">Tipo do fundo (para validações)</param>
    /// <param name="cnpjClasse">CNPJ próprio da classe (opcional)</param>
    /// <param name="tipoClasseFidc">Tipo de classe FIDC (obrigatório para FIDCs)</param>
    /// <param name="ordemSubordinacao">Ordem de subordinação (obrigatório para FIDCs)</param>
    /// <param name="rentabilidadeAlvo">Rentabilidade alvo % a.a.</param>
    /// <param name="responsabilidadeLimitada">Se possui responsabilidade limitada</param>
    /// <param name="segregacaoPatrimonial">Se possui segregação patrimonial</param>
    /// <param name="valorMinimoAplicacao">Valor mínimo de aplicação</param>
    /// <param name="permiteResgateAntecipado">Se permite resgate antecipado</param>
    public static FundoClasse Criar(
        Guid fundoId,
        string codigoClasse,
        string nomeClasse,
        TipoFundo tipoFundo,
        string? cnpjClasse = null,
        TipoClasseFIDC? tipoClasseFidc = null,
        int? ordemSubordinacao = null,
        decimal? rentabilidadeAlvo = null,
        bool responsabilidadeLimitada = false,
        bool segregacaoPatrimonial = false,
        decimal? valorMinimoAplicacao = null,
        bool permiteResgateAntecipado = true)
    {
        ValidarParametros(codigoClasse, nomeClasse, tipoFundo, tipoClasseFidc, ordemSubordinacao,
            rentabilidadeAlvo, valorMinimoAplicacao, cnpjClasse);

        return new FundoClasse
        {
            Id = Guid.NewGuid(),
            FundoId = fundoId,
            CnpjClasse = cnpjClasse?.Trim(),
            CodigoClasse = codigoClasse.Trim().ToUpperInvariant(),
            NomeClasse = nomeClasse.Trim(),
            TipoClasseFidc = tipoClasseFidc,
            OrdemSubordinacao = ordemSubordinacao,
            RentabilidadeAlvo = rentabilidadeAlvo,
            ResponsabilidadeLimitada = responsabilidadeLimitada,
            SegregacaoPatrimonial = segregacaoPatrimonial,
            ValorMinimoAplicacao = valorMinimoAplicacao,
            PermiteResgateAntecipado = permiteResgateAntecipado,
            Ativa = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza os dados da classe.
    /// </summary>
    public void Atualizar(
        string nomeClasse,
        string? cnpjClasse,
        TipoClasseFIDC? tipoClasseFidc,
        int? ordemSubordinacao,
        decimal? rentabilidadeAlvo,
        bool responsabilidadeLimitada,
        bool segregacaoPatrimonial,
        decimal? valorMinimoAplicacao,
        TipoFundo tipoFundo,
        bool permiteResgateAntecipado = true)
    {
        ValidarParametros(CodigoClasse, nomeClasse, tipoFundo, tipoClasseFidc, ordemSubordinacao,
            rentabilidadeAlvo, valorMinimoAplicacao, cnpjClasse);

        CnpjClasse = cnpjClasse?.Trim();
        NomeClasse = nomeClasse.Trim();
        TipoClasseFidc = tipoClasseFidc;
        OrdemSubordinacao = ordemSubordinacao;
        RentabilidadeAlvo = rentabilidadeAlvo;
        ResponsabilidadeLimitada = responsabilidadeLimitada;
        SegregacaoPatrimonial = segregacaoPatrimonial;
        ValorMinimoAplicacao = valorMinimoAplicacao;
        PermiteResgateAntecipado = permiteResgateAntecipado;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Encerra a classe com data e motivo.
    /// </summary>
    /// <param name="dataEncerramento">Data do encerramento.</param>
    /// <param name="motivoEncerramento">Motivo do encerramento.</param>
    public void Encerrar(DateOnly dataEncerramento, string? motivoEncerramento = null)
    {
        if (DataEncerramento.HasValue)
            throw new DomainValidationException("A classe já foi encerrada.");

        DataEncerramento = dataEncerramento;
        MotivoEncerramento = motivoEncerramento?.Trim();
        Ativa = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Reabre uma classe encerrada.
    /// </summary>
    public void Reabrir()
    {
        if (!DataEncerramento.HasValue)
            throw new DomainValidationException("A classe não está encerrada.");

        DataEncerramento = null;
        MotivoEncerramento = null;
        Ativa = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Ativa a classe.
    /// </summary>
    public void Ativar()
    {
        Ativa = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Desativa a classe.
    /// </summary>
    public void Desativar()
    {
        Ativa = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Marca a classe como excluída (soft delete).
    /// </summary>
    public void Excluir()
    {
        if (Subclasses.Any(s => s.DeletedAt == null))
            throw new DomainValidationException(
                "Não é possível excluir uma classe com subclasses ativas.");

        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Restaura uma classe excluída.
    /// </summary>
    public void Restaurar()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Verifica se o fundo permite classes (FI, FIC, FIDC).
    /// </summary>
    public static bool FundoPermiteClasses(TipoFundo tipoFundo)
    {
        return tipoFundo is TipoFundo.FI or TipoFundo.FIC or TipoFundo.FIDC or TipoFundo.FICFIDC;
    }

    /// <summary>
    ///     Verifica se o tipo de fundo é FIDC.
    /// </summary>
    public static bool EhFIDC(TipoFundo tipoFundo)
    {
        return tipoFundo is TipoFundo.FIDC or TipoFundo.FICFIDC;
    }

    private static void ValidarParametros(
        string codigoClasse,
        string nomeClasse,
        TipoFundo tipoFundo,
        TipoClasseFIDC? tipoClasseFidc,
        int? ordemSubordinacao,
        decimal? rentabilidadeAlvo,
        decimal? valorMinimoAplicacao,
        string? cnpjClasse)
    {
        if (string.IsNullOrWhiteSpace(codigoClasse))
            throw new DomainValidationException("Código da classe é obrigatório.");

        if (codigoClasse.Length > 10)
            throw new DomainValidationException("Código da classe deve ter no máximo 10 caracteres.");

        if (string.IsNullOrWhiteSpace(nomeClasse))
            throw new DomainValidationException("Nome da classe é obrigatório.");

        if (nomeClasse.Length > 100)
            throw new DomainValidationException("Nome da classe deve ter no máximo 100 caracteres.");

        if (cnpjClasse != null && cnpjClasse.Length > 14)
            throw new DomainValidationException("CNPJ da classe deve ter no máximo 14 caracteres.");

        // Validações específicas para FIDC
        if (EhFIDC(tipoFundo))
        {
            if (!tipoClasseFidc.HasValue)
                throw new DomainValidationException(
                    "Tipo de classe FIDC é obrigatório para fundos FIDC.");

            if (!ordemSubordinacao.HasValue)
                throw new DomainValidationException(
                    "Ordem de subordinação é obrigatória para fundos FIDC.");

            if (ordemSubordinacao < 1)
                throw new DomainValidationException(
                    "Ordem de subordinação deve ser maior que zero.");
        }

        if (rentabilidadeAlvo.HasValue && rentabilidadeAlvo < 0)
            throw new DomainValidationException(
                "Rentabilidade alvo não pode ser negativa.");

        if (valorMinimoAplicacao.HasValue && valorMinimoAplicacao < 0)
            throw new DomainValidationException(
                "Valor mínimo de aplicação não pode ser negativo.");
    }
}
