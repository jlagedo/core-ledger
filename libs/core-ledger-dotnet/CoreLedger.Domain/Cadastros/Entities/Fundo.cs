using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Entidade principal representando um Fundo de Investimento conforme CVM 175.
/// </summary>
public class Fundo
{
    /// <summary>
    ///     Identificador único do fundo (UUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     CNPJ do fundo (14 dígitos, sem formatação).
    /// </summary>
    public CNPJ Cnpj { get; private set; } = null!;

    /// <summary>
    ///     Razão social do fundo.
    /// </summary>
    public string RazaoSocial { get; private set; } = null!;

    /// <summary>
    ///     Nome fantasia do fundo.
    /// </summary>
    public string? NomeFantasia { get; private set; }

    /// <summary>
    ///     Nome curto para exibição.
    /// </summary>
    public string? NomeCurto { get; private set; }

    /// <summary>
    ///     Data de constituição do fundo.
    /// </summary>
    public DateOnly? DataConstituicao { get; private set; }

    /// <summary>
    ///     Data de início das atividades do fundo.
    /// </summary>
    public DateOnly? DataInicioAtividade { get; private set; }

    /// <summary>
    ///     Tipo do fundo (FI, FIC, FIDC, etc.).
    /// </summary>
    public TipoFundo TipoFundo { get; private set; }

    /// <summary>
    ///     Classificação CVM do fundo.
    /// </summary>
    public ClassificacaoCVM ClassificacaoCVM { get; private set; }

    /// <summary>
    ///     Classificação ANBIMA do fundo.
    /// </summary>
    public string? ClassificacaoAnbima { get; private set; }

    /// <summary>
    ///     Código ANBIMA do fundo (6 dígitos).
    /// </summary>
    public CodigoANBIMA? CodigoAnbima { get; private set; }

    /// <summary>
    ///     Situação atual do fundo.
    /// </summary>
    public SituacaoFundo Situacao { get; private set; }

    /// <summary>
    ///     Prazo do fundo (Determinado ou Indeterminado).
    /// </summary>
    public PrazoFundo Prazo { get; private set; }

    /// <summary>
    ///     Público-alvo do fundo.
    /// </summary>
    public PublicoAlvo PublicoAlvo { get; private set; }

    /// <summary>
    ///     Regime de tributação do fundo.
    /// </summary>
    public TributacaoFundo Tributacao { get; private set; }

    /// <summary>
    ///     Tipo de condomínio (Aberto ou Fechado).
    /// </summary>
    public TipoCondominio Condominio { get; private set; }

    /// <summary>
    ///     Indica se o fundo é exclusivo.
    /// </summary>
    public bool Exclusivo { get; private set; }

    /// <summary>
    ///     Indica se o fundo é reservado.
    /// </summary>
    public bool Reservado { get; private set; }

    /// <summary>
    ///     Indica se o fundo permite alavancagem.
    /// </summary>
    public bool PermiteAlavancagem { get; private set; }

    /// <summary>
    ///     Indica se o fundo aceita investimento em criptoativos.
    /// </summary>
    public bool AceitaCripto { get; private set; }

    /// <summary>
    ///     Percentual máximo de investimento no exterior (0-100).
    /// </summary>
    public decimal PercentualExterior { get; private set; }

    /// <summary>
    ///     Indica se o wizard de cadastro foi completado.
    /// </summary>
    public bool WizardCompleto { get; private set; }

    /// <summary>
    ///     Progresso do cadastro (0-100%).
    /// </summary>
    public int ProgressoCadastro { get; private set; }

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
    ///     Classes de cotas do fundo (relacionamento 1:N).
    /// </summary>
    public ICollection<FundoClasse> Classes { get; private set; } = new List<FundoClasse>();

    /// <summary>
    ///     Taxas do fundo (relacionamento 1:N).
    /// </summary>
    public ICollection<FundoTaxa> Taxas { get; private set; } = new List<FundoTaxa>();

    /// <summary>
    ///     Parâmetros específicos para FIDC (relacionamento 1:1, opcional).
    /// </summary>
    public FundoParametrosFIDC? ParametrosFIDC { get; private set; }

    /// <summary>
    ///     Parâmetros de cálculo e exibição da cota (relacionamento 1:1, opcional).
    /// </summary>
    public FundoParametrosCota? ParametrosCota { get; private set; }

    /// <summary>
    ///     Identificador do usuário que criou o registro.
    /// </summary>
    public string? CreatedBy { get; private set; }

    /// <summary>
    ///     Identificador do usuário que atualizou o registro pela última vez.
    /// </summary>
    public string? UpdatedBy { get; private set; }

    // Construtor privado para EF Core
    private Fundo()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de Fundo com validações.
    /// </summary>
    public static Fundo Criar(
        string cnpj,
        string razaoSocial,
        TipoFundo tipoFundo,
        ClassificacaoCVM classificacaoCVM,
        PrazoFundo prazo,
        PublicoAlvo publicoAlvo,
        TributacaoFundo tributacao,
        TipoCondominio condominio,
        string? nomeFantasia = null,
        string? nomeCurto = null,
        DateOnly? dataConstituicao = null,
        DateOnly? dataInicioAtividade = null,
        string? classificacaoAnbima = null,
        string? codigoAnbima = null,
        bool exclusivo = false,
        bool reservado = false,
        bool permiteAlavancagem = false,
        bool aceitaCripto = false,
        decimal percentualExterior = 0,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new DomainValidationException("Razão social é obrigatória.");

        if (razaoSocial.Length > 200)
            throw new DomainValidationException("Razão social deve ter no máximo 200 caracteres.");

        if (nomeFantasia?.Length > 100)
            throw new DomainValidationException("Nome fantasia deve ter no máximo 100 caracteres.");

        if (nomeCurto?.Length > 30)
            throw new DomainValidationException("Nome curto deve ter no máximo 30 caracteres.");

        if (percentualExterior < 0 || percentualExterior > 100)
            throw new DomainValidationException("Percentual exterior deve estar entre 0 e 100.");

        if (dataInicioAtividade.HasValue && dataConstituicao.HasValue &&
            dataInicioAtividade < dataConstituicao)
            throw new DomainValidationException(
                "Data de início de atividade não pode ser anterior à data de constituição.");

        var cnpjVO = CNPJ.Criar(cnpj);
        CodigoANBIMA? codigoAnbimaVO = null;

        if (!string.IsNullOrWhiteSpace(codigoAnbima))
            codigoAnbimaVO = CodigoANBIMA.Criar(codigoAnbima);

        return new Fundo
        {
            Id = Guid.NewGuid(),
            Cnpj = cnpjVO,
            RazaoSocial = razaoSocial.Trim(),
            NomeFantasia = nomeFantasia?.Trim(),
            NomeCurto = nomeCurto?.Trim(),
            DataConstituicao = dataConstituicao,
            DataInicioAtividade = dataInicioAtividade,
            TipoFundo = tipoFundo,
            ClassificacaoCVM = classificacaoCVM,
            ClassificacaoAnbima = classificacaoAnbima?.Trim(),
            CodigoAnbima = codigoAnbimaVO,
            Situacao = SituacaoFundo.EmConstituicao,
            Prazo = prazo,
            PublicoAlvo = publicoAlvo,
            Tributacao = tributacao,
            Condominio = condominio,
            Exclusivo = exclusivo,
            Reservado = reservado,
            PermiteAlavancagem = permiteAlavancagem,
            AceitaCripto = aceitaCripto,
            PercentualExterior = percentualExterior,
            WizardCompleto = false,
            ProgressoCadastro = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    ///     Atualiza os dados cadastrais do fundo.
    /// </summary>
    public void AtualizarDadosCadastrais(
        string razaoSocial,
        string? nomeFantasia,
        string? nomeCurto,
        DateOnly? dataConstituicao,
        DateOnly? dataInicioAtividade,
        string? classificacaoAnbima,
        string? codigoAnbima,
        string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new DomainValidationException("Razão social é obrigatória.");

        if (razaoSocial.Length > 200)
            throw new DomainValidationException("Razão social deve ter no máximo 200 caracteres.");

        if (nomeFantasia?.Length > 100)
            throw new DomainValidationException("Nome fantasia deve ter no máximo 100 caracteres.");

        if (nomeCurto?.Length > 30)
            throw new DomainValidationException("Nome curto deve ter no máximo 30 caracteres.");

        if (dataInicioAtividade.HasValue && dataConstituicao.HasValue &&
            dataInicioAtividade < dataConstituicao)
            throw new DomainValidationException(
                "Data de início de atividade não pode ser anterior à data de constituição.");

        CodigoANBIMA? codigoAnbimaVO = null;
        if (!string.IsNullOrWhiteSpace(codigoAnbima))
            codigoAnbimaVO = CodigoANBIMA.Criar(codigoAnbima);

        RazaoSocial = razaoSocial.Trim();
        NomeFantasia = nomeFantasia?.Trim();
        NomeCurto = nomeCurto?.Trim();
        DataConstituicao = dataConstituicao;
        DataInicioAtividade = dataInicioAtividade;
        ClassificacaoAnbima = classificacaoAnbima?.Trim();
        CodigoAnbima = codigoAnbimaVO;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    ///     Atualiza as configurações regulatórias do fundo.
    /// </summary>
    public void AtualizarConfiguracoesRegulatorias(
        ClassificacaoCVM classificacaoCVM,
        PrazoFundo prazo,
        PublicoAlvo publicoAlvo,
        TributacaoFundo tributacao,
        TipoCondominio condominio,
        bool exclusivo,
        bool reservado,
        bool permiteAlavancagem,
        bool aceitaCripto,
        decimal percentualExterior,
        string? updatedBy = null)
    {
        if (percentualExterior < 0 || percentualExterior > 100)
            throw new DomainValidationException("Percentual exterior deve estar entre 0 e 100.");

        ClassificacaoCVM = classificacaoCVM;
        Prazo = prazo;
        PublicoAlvo = publicoAlvo;
        Tributacao = tributacao;
        Condominio = condominio;
        Exclusivo = exclusivo;
        Reservado = reservado;
        PermiteAlavancagem = permiteAlavancagem;
        AceitaCripto = aceitaCripto;
        PercentualExterior = percentualExterior;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    ///     Altera a situação do fundo.
    /// </summary>
    public void AlterarSituacao(SituacaoFundo novaSituacao, string? updatedBy = null)
    {
        ValidarTransicaoSituacao(novaSituacao);
        Situacao = novaSituacao;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    ///     Atualiza o progresso do cadastro.
    /// </summary>
    public void AtualizarProgresso(int progresso, string? updatedBy = null)
    {
        if (progresso < 0 || progresso > 100)
            throw new DomainValidationException("Progresso deve estar entre 0 e 100.");

        ProgressoCadastro = progresso;
        WizardCompleto = progresso == 100;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    ///     Marca o fundo como excluído (soft delete).
    /// </summary>
    public void Excluir(string? deletedBy = null)
    {
        if (Situacao == SituacaoFundo.Ativo)
            throw new DomainValidationException(
                "Não é possível excluir um fundo ativo. Altere a situação primeiro.");

        DeletedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }

    /// <summary>
    ///     Restaura um fundo excluído.
    /// </summary>
    public void Restaurar(string? updatedBy = null)
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    private void ValidarTransicaoSituacao(SituacaoFundo novaSituacao)
    {
        // Regras de transição de situação
        var transicaoValida = (Situacao, novaSituacao) switch
        {
            (SituacaoFundo.EmConstituicao, SituacaoFundo.Ativo) => true,
            (SituacaoFundo.Ativo, SituacaoFundo.Suspenso) => true,
            (SituacaoFundo.Ativo, SituacaoFundo.EmLiquidacao) => true,
            (SituacaoFundo.Suspenso, SituacaoFundo.Ativo) => true,
            (SituacaoFundo.Suspenso, SituacaoFundo.EmLiquidacao) => true,
            (SituacaoFundo.EmLiquidacao, SituacaoFundo.Liquidado) => true,
            _ when Situacao == novaSituacao => true, // Mesma situação é válido
            _ => false
        };

        if (!transicaoValida)
            throw new DomainValidationException(
                $"Transição de situação inválida: {Situacao} -> {novaSituacao}.");
    }
}
