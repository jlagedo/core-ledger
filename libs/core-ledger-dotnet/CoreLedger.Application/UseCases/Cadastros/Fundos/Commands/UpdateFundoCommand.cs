using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Domain.Cadastros.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;

/// <summary>
///     Command to update an existing Fundo.
/// </summary>
public record UpdateFundoCommand(
    Guid Id,
    string RazaoSocial,
    string? NomeFantasia = null,
    string? NomeCurto = null,
    DateOnly? DataConstituicao = null,
    DateOnly? DataInicioAtividade = null,
    ClassificacaoCVM? ClassificacaoCVM = null,
    string? ClassificacaoAnbima = null,
    string? CodigoAnbima = null,
    PrazoFundo? Prazo = null,
    PublicoAlvo? PublicoAlvo = null,
    TributacaoFundo? Tributacao = null,
    TipoCondominio? Condominio = null,
    bool? Exclusivo = null,
    bool? Reservado = null,
    bool? PermiteAlavancagem = null,
    bool? AceitaCripto = null,
    decimal? PercentualExterior = null,
    string? UpdatedBy = null
) : IRequest<FundoResponseDto>;
