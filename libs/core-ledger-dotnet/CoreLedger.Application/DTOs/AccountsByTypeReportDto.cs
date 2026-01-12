namespace CoreLedger.Application.DTOs;

public record AccountsByTypeReportDto(
    int TypeId,
    string TypeDescription,
    int ActiveAccountCount
);