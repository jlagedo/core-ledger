namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for ClassificacaoAnbima entity.
/// </summary>
public record ClassificacaoAnbimaDto(
    int Id,
    string Codigo,
    string Nome,
    string Nivel1,
    string Nivel2,
    string? Nivel3,
    string ClassificacaoCvm,
    string? Descricao,
    string NomeCompleto
);

/// <summary>
///     Response DTO for listing ANBIMA classifications with filtering information.
/// </summary>
public record ListarClassificacoesAnbimaResponse(
    List<ClassificacaoAnbimaDto> Items,
    int Total,
    string? ClassificacaoCvmFiltrada,
    string? Mensagem
);

/// <summary>
///     DTO representing a count of items at a specific hierarchical level.
/// </summary>
public record NivelContagem(
    string Valor,
    int Quantidade
);

/// <summary>
///     Response DTO for hierarchical classification levels (Nivel1, Nivel2 grouped by Nivel1).
/// </summary>
public record NiveisClassificacaoAnbimaResponse(
    List<NivelContagem> Nivel1,
    Dictionary<string, List<NivelContagem>> Nivel2PorNivel1
);

/// <summary>
///     Response DTO for verifying compatibility between ANBIMA classification and CVM classification.
/// </summary>
public record VerificarCompatibilidadeResponse(
    bool Compativel,
    string CodigoAnbima,
    string ClassificacaoCvm,
    string? Mensagem
);
