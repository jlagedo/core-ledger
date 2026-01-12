using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Securities.Commands;

/// <summary>
///     Command to create a new Security.
/// </summary>
public record CreateSecurityCommand(
    string Name,
    string Ticker,
    string? Isin,
    SecurityType Type,
    string Currency,
    string CreatedByUserId
) : IRequest<SecurityDto>;