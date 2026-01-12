using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Vinculos.Commands;

/// <summary>
///     Command to encerrar (end) a FundoVinculo.
/// </summary>
public record EncerrarVinculoCommand(long Id, DateOnly DataFim) : IRequest<Unit>;
