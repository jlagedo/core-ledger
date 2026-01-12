namespace CoreLedger.Application.DTOs.Fundo;

/// <summary>
///     Response DTO for CNPJ availability check.
/// </summary>
public record CnpjDisponibilidadeResponseDto(
    string Cnpj,
    bool Disponivel,
    bool CnpjValido,
    string? Mensagem = null);
