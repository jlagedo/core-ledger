using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.Entities;

/// <summary>
///     Entidade representando o vínculo entre um fundo e uma instituição financeira.
/// </summary>
public class FundoVinculo
{
    /// <summary>
    ///     Identificador único do vínculo (BIGINT SERIAL).
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Identificador do fundo.
    /// </summary>
    public Guid FundoId { get; private set; }

    /// <summary>
    ///     Fundo associado ao vínculo (navegação).
    /// </summary>
    public Fundo Fundo { get; private set; } = null!;

    /// <summary>
    ///     Identificador da instituição.
    /// </summary>
    public int InstituicaoId { get; private set; }

    /// <summary>
    ///     Instituição associada ao vínculo (navegação).
    /// </summary>
    public Instituicao Instituicao { get; private set; } = null!;

    /// <summary>
    ///     Tipo de vínculo institucional.
    /// </summary>
    public TipoVinculoInstitucional TipoVinculo { get; private set; }

    /// <summary>
    ///     Data de início do vínculo.
    /// </summary>
    public DateOnly DataInicio { get; private set; }

    /// <summary>
    ///     Data de fim do vínculo (NULL = vigente).
    /// </summary>
    public DateOnly? DataFim { get; private set; }

    /// <summary>
    ///     Número do contrato do vínculo.
    /// </summary>
    public string? ContratoNumero { get; private set; }

    /// <summary>
    ///     Observações sobre o vínculo.
    /// </summary>
    public string? Observacao { get; private set; }

    /// <summary>
    ///     Indica se este é o vínculo principal do tipo.
    /// </summary>
    public bool Principal { get; private set; }

    /// <summary>
    ///     Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Data e hora da última atualização do registro.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    // Construtor privado para EF Core
    private FundoVinculo()
    {
    }

    /// <summary>
    ///     Cria uma nova instância de FundoVinculo com validações.
    /// </summary>
    public static FundoVinculo Criar(
        Guid fundoId,
        int instituicaoId,
        TipoVinculoInstitucional tipoVinculo,
        DateOnly dataInicio,
        bool principal = false,
        string? contratoNumero = null,
        string? observacao = null)
    {
        if (fundoId == Guid.Empty)
            throw new DomainValidationException("FundoId é obrigatório.");

        if (instituicaoId <= 0)
            throw new DomainValidationException("InstituicaoId é obrigatório.");

        if (contratoNumero?.Length > 50)
            throw new DomainValidationException("Número do contrato deve ter no máximo 50 caracteres.");

        if (observacao?.Length > 500)
            throw new DomainValidationException("Observação deve ter no máximo 500 caracteres.");

        return new FundoVinculo
        {
            FundoId = fundoId,
            InstituicaoId = instituicaoId,
            TipoVinculo = tipoVinculo,
            DataInicio = dataInicio,
            DataFim = null,
            ContratoNumero = contratoNumero?.Trim(),
            Observacao = observacao?.Trim(),
            Principal = principal,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Atualiza os dados do vínculo.
    /// </summary>
    public void Atualizar(
        DateOnly dataInicio,
        bool principal,
        string? contratoNumero = null,
        string? observacao = null)
    {
        if (DataFim.HasValue)
            throw new DomainValidationException("Não é possível atualizar um vínculo encerrado.");

        if (contratoNumero?.Length > 50)
            throw new DomainValidationException("Número do contrato deve ter no máximo 50 caracteres.");

        if (observacao?.Length > 500)
            throw new DomainValidationException("Observação deve ter no máximo 500 caracteres.");

        DataInicio = dataInicio;
        ContratoNumero = contratoNumero?.Trim();
        Observacao = observacao?.Trim();
        Principal = principal;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Encerra o vínculo com uma data de fim.
    /// </summary>
    public void Encerrar(DateOnly dataFim)
    {
        if (DataFim.HasValue)
            throw new DomainValidationException("Vínculo já está encerrado.");

        if (dataFim < DataInicio)
            throw new DomainValidationException("Data de fim não pode ser anterior à data de início.");

        DataFim = dataFim;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Marca ou desmarca este vínculo como principal.
    /// </summary>
    public void DefinirComoPrincipal(bool principal)
    {
        if (DataFim.HasValue && principal)
            throw new DomainValidationException("Não é possível marcar um vínculo encerrado como principal.");

        Principal = principal;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Verifica se o vínculo está vigente (sem data de fim).
    /// </summary>
    public bool EstaVigente() => !DataFim.HasValue;
}
