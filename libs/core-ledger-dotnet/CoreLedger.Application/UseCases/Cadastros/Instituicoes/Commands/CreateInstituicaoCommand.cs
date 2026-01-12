using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Instituicoes.Commands;

/// <summary>
///     Command to create a new Instituição.
/// </summary>
public record CreateInstituicaoCommand(
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    bool Ativo = true
) : IRequest<InstituicaoDto>;
