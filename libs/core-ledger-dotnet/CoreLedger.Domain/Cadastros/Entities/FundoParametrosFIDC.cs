using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Parâmetros específicos para Fundos de Investimento em Direitos Creditórios (FIDC).
///     Relacionamento 1:1 com Fundo. Aplicável apenas para fundos tipo FIDC ou FICFIDC.
/// </summary>
public class FundoParametrosFIDC
{
    /// <summary>
    ///     Identificador único (BIGINT SERIAL).
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Identificador do fundo (relacionamento 1:1).
    /// </summary>
    public Guid FundoId { get; private set; }

    /// <summary>
    ///     Tipo do FIDC (Padronizado ou Não Padronizado).
    /// </summary>
    public TipoFIDC TipoFidc { get; private set; }

    /// <summary>
    ///     Lista de tipos de recebíveis aceitos pelo FIDC (armazenado como JSON).
    /// </summary>
    public List<TipoRecebiveis> TiposRecebiveis { get; private set; } = new();

    /// <summary>
    ///     Prazo médio da carteira em dias.
    /// </summary>
    public int? PrazoMedioCarteira { get; private set; }

    /// <summary>
    ///     Índice de subordinação alvo (%).
    /// </summary>
    public decimal? IndiceSubordinacaoAlvo { get; private set; }

    /// <summary>
    ///     Índice de subordinação mínimo (%).
    /// </summary>
    public decimal? IndiceSubordinacaoMinimo { get; private set; }

    /// <summary>
    ///     Provisão para devedores duvidosos (%).
    /// </summary>
    public decimal? ProvisaoDevedoresDuvidosos { get; private set; }

    /// <summary>
    ///     Limite de concentração por cedente (%).
    /// </summary>
    public decimal? LimiteConcentracaoCedente { get; private set; }

    /// <summary>
    ///     Limite de concentração por sacado (%).
    /// </summary>
    public decimal? LimiteConcentracaoSacado { get; private set; }

    /// <summary>
    ///     Indica se o fundo possui coobrigação.
    /// </summary>
    public bool PossuiCoobrigacao { get; private set; }

    /// <summary>
    ///     Percentual de coobrigação (%).
    /// </summary>
    public decimal? PercentualCoobrigacao { get; private set; }

    /// <summary>
    ///     Registradora de recebíveis utilizada.
    /// </summary>
    public Registradora? RegistradoraRecebiveis { get; private set; }

    /// <summary>
    ///     Indica se possui integração com a registradora.
    /// </summary>
    public bool IntegracaoRegistradora { get; private set; }

    /// <summary>
    ///     Código do fundo no sistema da registradora.
    /// </summary>
    public string? CodigoRegistradora { get; private set; }

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Data e hora da última atualização do registro.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    ///     Referência de navegação para o Fundo.
    /// </summary>
    public Fundo Fundo { get; private set; } = null!;

    // Construtor privado para EF Core
    private FundoParametrosFIDC()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de FundoParametrosFIDC com validações.
    /// </summary>
    public static FundoParametrosFIDC Criar(
        Guid fundoId,
        TipoFIDC tipoFidc,
        List<TipoRecebiveis> tiposRecebiveis,
        int? prazoMedioCarteira = null,
        decimal? indiceSubordinacaoAlvo = null,
        decimal? indiceSubordinacaoMinimo = null,
        decimal? provisaoDevedoresDuvidosos = null,
        decimal? limiteConcentracaoCedente = null,
        decimal? limiteConcentracaoSacado = null,
        bool possuiCoobrigacao = false,
        decimal? percentualCoobrigacao = null,
        Registradora? registradoraRecebiveis = null,
        bool integracaoRegistradora = false,
        string? codigoRegistradora = null)
    {
        // Validações
        if (tiposRecebiveis == null || tiposRecebiveis.Count == 0)
            throw new DomainValidationException("Tipos de recebíveis são obrigatórios.");

        if (prazoMedioCarteira.HasValue && prazoMedioCarteira.Value <= 0)
            throw new DomainValidationException("Prazo médio da carteira deve ser maior que zero.");

        ValidarPercentual(indiceSubordinacaoAlvo, "Índice de subordinação alvo");
        ValidarPercentual(indiceSubordinacaoMinimo, "Índice de subordinação mínimo");
        ValidarPercentual(provisaoDevedoresDuvidosos, "Provisão para devedores duvidosos");
        ValidarPercentual(limiteConcentracaoCedente, "Limite de concentração por cedente");
        ValidarPercentual(limiteConcentracaoSacado, "Limite de concentração por sacado");
        ValidarPercentual(percentualCoobrigacao, "Percentual de coobrigação");

        if (possuiCoobrigacao && (!percentualCoobrigacao.HasValue || percentualCoobrigacao.Value <= 0))
            throw new DomainValidationException(
                "Percentual de coobrigação é obrigatório quando possui coobrigação.");

        if (integracaoRegistradora && !registradoraRecebiveis.HasValue)
            throw new DomainValidationException(
                "Registradora de recebíveis é obrigatória quando há integração.");

        if (!string.IsNullOrWhiteSpace(codigoRegistradora) && codigoRegistradora.Length > 50)
            throw new DomainValidationException("Código da registradora deve ter no máximo 50 caracteres.");

        return new FundoParametrosFIDC
        {
            FundoId = fundoId,
            TipoFidc = tipoFidc,
            TiposRecebiveis = tiposRecebiveis,
            PrazoMedioCarteira = prazoMedioCarteira,
            IndiceSubordinacaoAlvo = indiceSubordinacaoAlvo,
            IndiceSubordinacaoMinimo = indiceSubordinacaoMinimo,
            ProvisaoDevedoresDuvidosos = provisaoDevedoresDuvidosos,
            LimiteConcentracaoCedente = limiteConcentracaoCedente,
            LimiteConcentracaoSacado = limiteConcentracaoSacado,
            PossuiCoobrigacao = possuiCoobrigacao,
            PercentualCoobrigacao = percentualCoobrigacao,
            RegistradoraRecebiveis = registradoraRecebiveis,
            IntegracaoRegistradora = integracaoRegistradora,
            CodigoRegistradora = codigoRegistradora?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza os parâmetros do FIDC.
    /// </summary>
    public void Atualizar(
        TipoFIDC tipoFidc,
        List<TipoRecebiveis> tiposRecebiveis,
        int? prazoMedioCarteira,
        decimal? indiceSubordinacaoAlvo,
        decimal? indiceSubordinacaoMinimo,
        decimal? provisaoDevedoresDuvidosos,
        decimal? limiteConcentracaoCedente,
        decimal? limiteConcentracaoSacado,
        bool possuiCoobrigacao,
        decimal? percentualCoobrigacao,
        Registradora? registradoraRecebiveis,
        bool integracaoRegistradora,
        string? codigoRegistradora)
    {
        if (tiposRecebiveis == null || tiposRecebiveis.Count == 0)
            throw new DomainValidationException("Tipos de recebíveis são obrigatórios.");

        if (prazoMedioCarteira.HasValue && prazoMedioCarteira.Value <= 0)
            throw new DomainValidationException("Prazo médio da carteira deve ser maior que zero.");

        ValidarPercentual(indiceSubordinacaoAlvo, "Índice de subordinação alvo");
        ValidarPercentual(indiceSubordinacaoMinimo, "Índice de subordinação mínimo");
        ValidarPercentual(provisaoDevedoresDuvidosos, "Provisão para devedores duvidosos");
        ValidarPercentual(limiteConcentracaoCedente, "Limite de concentração por cedente");
        ValidarPercentual(limiteConcentracaoSacado, "Limite de concentração por sacado");
        ValidarPercentual(percentualCoobrigacao, "Percentual de coobrigação");

        if (possuiCoobrigacao && (!percentualCoobrigacao.HasValue || percentualCoobrigacao.Value <= 0))
            throw new DomainValidationException(
                "Percentual de coobrigação é obrigatório quando possui coobrigação.");

        if (integracaoRegistradora && !registradoraRecebiveis.HasValue)
            throw new DomainValidationException(
                "Registradora de recebíveis é obrigatória quando há integração.");

        if (!string.IsNullOrWhiteSpace(codigoRegistradora) && codigoRegistradora.Length > 50)
            throw new DomainValidationException("Código da registradora deve ter no máximo 50 caracteres.");

        TipoFidc = tipoFidc;
        TiposRecebiveis = tiposRecebiveis;
        PrazoMedioCarteira = prazoMedioCarteira;
        IndiceSubordinacaoAlvo = indiceSubordinacaoAlvo;
        IndiceSubordinacaoMinimo = indiceSubordinacaoMinimo;
        ProvisaoDevedoresDuvidosos = provisaoDevedoresDuvidosos;
        LimiteConcentracaoCedente = limiteConcentracaoCedente;
        LimiteConcentracaoSacado = limiteConcentracaoSacado;
        PossuiCoobrigacao = possuiCoobrigacao;
        PercentualCoobrigacao = percentualCoobrigacao;
        RegistradoraRecebiveis = registradoraRecebiveis;
        IntegracaoRegistradora = integracaoRegistradora;
        CodigoRegistradora = codigoRegistradora?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidarPercentual(decimal? valor, string nomeCampo)
    {
        if (valor.HasValue && (valor.Value < 0 || valor.Value > 1))
            throw new DomainValidationException($"{nomeCampo} deve estar entre 0 e 1 (0-100%).");
    }
}
