using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Funds.Commands;

/// <summary>
///     Command to update an existing Fund.
/// </summary>
public record UpdateFundCommand(
    int Id,
    string Code,
    string Name,
    string BaseCurrency,
    DateTime InceptionDate,
    ValuationFrequency ValuationFrequency
) : IRequest;