using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Funds.Commands;

/// <summary>
///     Command to create a new Fund.
/// </summary>
public record CreateFundCommand(
    string Code,
    string Name,
    string BaseCurrency,
    DateTime InceptionDate,
    ValuationFrequency ValuationFrequency,
    string CreatedByUserId
) : IRequest<FundDto>;