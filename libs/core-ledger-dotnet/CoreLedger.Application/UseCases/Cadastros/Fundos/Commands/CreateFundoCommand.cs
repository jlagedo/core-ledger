using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Domain.Cadastros.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;

/// <summary>
///     Command to create a new Fundo.
/// </summary>
public record CreateFundoCommand(
    string Cnpj,
    string RazaoSocial,
    TipoFundo TipoFundo,
    ClassificacaoCVM ClassificacaoCVM,
    PrazoFundo Prazo,
    PublicoAlvo PublicoAlvo,
    TributacaoFundo Tributacao,
    TipoCondominio Condominio,
    string? NomeFantasia = null,
    string? NomeCurto = null,
    DateOnly? DataConstituicao = null,
    DateOnly? DataInicioAtividade = null,
    string? ClassificacaoAnbima = null,
    string? CodigoAnbima = null,
    bool Exclusivo = false,
    bool Reservado = false,
    bool PermiteAlavancagem = false,
    bool AceitaCripto = false,
    decimal PercentualExterior = 0,
    string? CreatedBy = null
) : IRequest<FundoResponseDto>;
