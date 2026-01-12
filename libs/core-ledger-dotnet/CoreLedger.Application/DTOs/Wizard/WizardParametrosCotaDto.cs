using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Cadastros.Enums;

namespace CoreLedger.Application.DTOs.Wizard;

/// <summary>
///     Seção de parâmetros de cota do fundo no wizard.
/// </summary>
public record WizardParametrosCotaDto(
    /// <summary>
    ///     Tipo de cálculo da cota (Fechamento ou Abertura).
    /// </summary>
    [Required(ErrorMessage = "Tipo de cota é obrigatório.")]
    TipoCota TipoCota,

    /// <summary>
    ///     Horário de corte para movimentações (HH:mm:ss).
    /// </summary>
    [Required(ErrorMessage = "Horário de corte é obrigatório.")]
    TimeOnly HorarioCorte,

    /// <summary>
    ///     Valor da cota na constituição do fundo.
    /// </summary>
    [Required(ErrorMessage = "Cota inicial é obrigatória.")]
    [Range(0.00000001, 1000000, ErrorMessage = "Cota inicial deve estar entre 0,00000001 e 1.000.000.")]
    decimal CotaInicial,

    /// <summary>
    ///     Data da primeira cota do fundo.
    /// </summary>
    [Required(ErrorMessage = "Data da cota inicial é obrigatória.")]
    DateOnly DataCotaInicial,

    /// <summary>
    ///     Número de casas decimais para a cota (4-10, padrão: 8).
    /// </summary>
    [Range(4, 10, ErrorMessage = "Casas decimais da cota deve estar entre 4 e 10.")]
    int CasasDecimaisCota = 8,

    /// <summary>
    ///     Número de casas decimais para quantidade de cotas (4-8, padrão: 6).
    /// </summary>
    [Range(4, 8, ErrorMessage = "Casas decimais da quantidade deve estar entre 4 e 8.")]
    int CasasDecimaisQuantidade = 6,

    /// <summary>
    ///     Número de casas decimais para o patrimônio líquido (2-4, padrão: 2).
    /// </summary>
    [Range(2, 4, ErrorMessage = "Casas decimais do PL deve estar entre 2 e 4.")]
    int CasasDecimaisPl = 2,

    /// <summary>
    ///     Fuso horário para cálculos (padrão: America/Sao_Paulo).
    /// </summary>
    [StringLength(50, ErrorMessage = "Fuso horário deve ter no máximo 50 caracteres.")]
    string FusoHorario = "America/Sao_Paulo",

    /// <summary>
    ///     Indica se permite divulgação de cota estimada antes do fechamento oficial.
    /// </summary>
    bool PermiteCotaEstimada = false
);
