using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Entidade de domínio Calendário representando dias úteis e feriados para cálculos D+X.
/// </summary>
public class Calendario : BaseEntity
{
    private Calendario() { }

    /// <summary>
    ///     A data do calendário.
    /// </summary>
    public DateOnly Data { get; private set; }

    /// <summary>
    ///     Indica se a data é um dia útil.
    ///     Calculado automaticamente a partir de TipoDia (CAL-004).
    /// </summary>
    public bool DiaUtil { get; private set; }

    /// <summary>
    ///     O tipo de classificação do dia.
    /// </summary>
    public TipoDia TipoDia { get; private set; }

    /// <summary>
    ///     A localização de mercado (praça) para este registro de calendário.
    /// </summary>
    public Praca Praca { get; private set; }

    /// <summary>
    ///     Descrição opcional (ex: nome do feriado).
    /// </summary>
    public string? Descricao { get; private set; }

    /// <summary>
    ///     ID do usuário que criou este registro de calendário.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Método factory para criar um novo Calendário com validação.
    /// </summary>
    /// <param name="data">A data do calendário.</param>
    /// <param name="tipoDia">O tipo de dia.</param>
    /// <param name="praca">A localização de mercado.</param>
    /// <param name="descricao">Descrição opcional (nome do feriado).</param>
    /// <param name="createdByUserId">ID do usuário que está criando este registro.</param>
    /// <returns>Uma nova instância de Calendário.</returns>
    public static Calendario Create(
        DateOnly data,
        TipoDia tipoDia,
        Praca praca,
        string? descricao,
        string createdByUserId)
    {
        ValidateData(data);
        ValidateTipoDia(tipoDia);
        ValidatePraca(praca);
        ValidateDescricao(descricao);
        ValidateCreatedByUserId(createdByUserId);

        return new Calendario
        {
            Data = data,
            TipoDia = tipoDia,
            DiaUtil = ComputeDiaUtil(tipoDia), // CAL-004: Auto-calcular a partir de TipoDia
            Praca = praca,
            Descricao = descricao?.Trim(),
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Atualiza o registro do calendário.
    /// </summary>
    /// <param name="tipoDia">O tipo de dia.</param>
    /// <param name="descricao">Descrição opcional.</param>
    public void Update(TipoDia tipoDia, string? descricao)
    {
        ValidateTipoDia(tipoDia);
        ValidateDescricao(descricao);

        TipoDia = tipoDia;
        DiaUtil = ComputeDiaUtil(tipoDia); // CAL-004: Auto-calcular a partir de TipoDia
        Descricao = descricao?.Trim();
        SetUpdated();
    }

    /// <summary>
    ///     Calcula se o dia é um dia útil com base em TipoDia (CAL-004).
    /// </summary>
    private static bool ComputeDiaUtil(TipoDia tipoDia)
    {
        return tipoDia == TipoDia.Util;
    }

    private static void ValidateData(DateOnly data)
    {
        if (data == default)
        {
            throw new DomainValidationException("Data não pode ser valor padrão");
        }

        if (data.Year < 1900 || data.Year > 2100)
        {
            throw new DomainValidationException("Ano da data deve estar entre 1900 e 2100");
        }
    }

    private static void ValidateTipoDia(TipoDia tipoDia)
    {
        if (!Enum.IsDefined(typeof(TipoDia), tipoDia))
        {
            throw new DomainValidationException("TipoDia deve ser um valor de enum válido");
        }
    }

    private static void ValidatePraca(Praca praca)
    {
        if (!Enum.IsDefined(typeof(Praca), praca))
        {
            throw new DomainValidationException("Praca deve ser um valor de enum válido");
        }
    }

    private static void ValidateDescricao(string? descricao)
    {
        if (descricao != null && descricao.Length > 100)
        {
            throw new DomainValidationException("Descricao não pode exceder 100 caracteres");
        }
    }

    private static void ValidateCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
        {
            throw new DomainValidationException("CreatedByUserId é obrigatório");
        }

        if (createdByUserId.Length > 200)
        {
            throw new DomainValidationException("CreatedByUserId não pode exceder 200 caracteres");
        }
    }
}
