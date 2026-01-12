using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Securities.Commands;

/// <summary>
///     Command to update an existing Security.
/// </summary>
public record UpdateSecurityCommand(
    int Id,
    string Name,
    string Ticker,
    string? Isin,
    SecurityType Type,
    string Currency
) : IRequest;