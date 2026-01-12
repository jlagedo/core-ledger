using CoreLedger.Application.DTOs.Wizard;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;

/// <summary>
///     Command para criar um fundo completo via wizard.
///     Cria o fundo com todas as entidades relacionadas em uma única transação atômica.
/// </summary>
public record CriarFundoWizardCommand(
    /// <summary>
    ///     DTO com todos os dados do wizard.
    /// </summary>
    FundoWizardRequestDto Request,

    /// <summary>
    ///     Identificador do usuário que está criando o fundo.
    /// </summary>
    string? CreatedBy = null
) : IRequest<FundoWizardResponseDto>;
