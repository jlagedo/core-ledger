using MediatR;

namespace CoreLedger.Application.UseCases.Cadastros.Taxas.Commands;

/// <summary>
///     Command to delete (deactivate) a FundoTaxa.
/// </summary>
public record DeleteTaxaCommand(long Id) : IRequest<Unit>;
