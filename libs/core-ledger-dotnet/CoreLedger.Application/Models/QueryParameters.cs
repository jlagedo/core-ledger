using System.Text.RegularExpressions;
using CoreLedger.Application.Configuration;

namespace CoreLedger.Application.Models;

/// <summary>
///     Parâmetros de consulta para operações GET em conformidade com RFC-8040 com validação abrangente.
/// </summary>
public class QueryParameters
{
    private string? _filter;
    private int _limit;
    private int _offset;
    private string _sortDirection = "asc";

    public QueryParameters()
    {
        _limit = PaginationDefaults.DefaultPageSize;
    }

    /// <summary>
    ///     Número máximo de itens a retornar (limite configurável, mínimo: 1).
    ///     Automaticamente limitado ao intervalo válido [1, MaxPageSize].
    /// </summary>
    public int Limit
    {
        get => _limit;
        set => _limit =
            value < 1 ? PaginationDefaults.DefaultPageSize : Math.Min(value, PaginationDefaults.MaxPageSize);
    }

    /// <summary>
    ///     Número de itens a pular (para paginação).
    ///     Automaticamente limitado a um mínimo de 0.
    /// </summary>
    public int Offset
    {
        get => _offset;
        set => _offset = Math.Max(value, 0);
    }

    /// <summary>
    ///     Campo a ordenar. Apenas campos listados em repositórios são usados.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    ///     Direção de ordenação (asc ou desc). Automaticamente normalizado para minúscula.
    ///     Valores inválidos padrão para "asc".
    /// </summary>
    public string SortDirection
    {
        get => _sortDirection;
        set => _sortDirection = ValidateSortDirection(value);
    }

    /// <summary>
    ///     Expressão de filtro (formato simples campo=valor).
    ///     Validado para prevenir tentativas de injeção SQL.
    /// </summary>
    public string? Filter
    {
        get => _filter;
        set => _filter = ValidateFilter(value);
    }

    private static string ValidateSortDirection(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "asc";

        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "asc" => "asc",
            "desc" => "desc",
            _ => "asc" // Padrão para ascendente para valores inválidos
        };
    }

    private static string? ValidateFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        // Verifica padrões de injeção SQL
        var dangerousPatterns = new[]
        {
            @"('|(--)|;|\/\*|\*\/|xp_|sp_|exec|execute|declare|create|drop|alter|insert|update|delete|union|select|cast|convert)",
            @"(@@|@[a-z]+)", // Variáveis do SQL Server
            @"(\bor\b|\band\b).*=.*", // OR/AND com igualdade (possível SQLi)
            @"(<script|<iframe|javascript:|onerror=|onload=)" // Tentativas de XSS
        };

        foreach (var pattern in dangerousPatterns)
            if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                throw new ArgumentException($"Filtro contém padrão potencialmente perigoso: {pattern}");

        // Garante que o filtro segue o formato campo=valor
        if (!Regex.IsMatch(value, @"^[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*.+$"))
            throw new ArgumentException("Filtro deve estar no formato 'campo=valor' com nome de campo válido");

        return value.Trim();
    }
}

/// <summary>
///     Invólucro de resultado paginado para conformidade com RFC-8040.
/// </summary>
/// <typeparam name="T">Tipo de itens no resultado</typeparam>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Limit,
    int Offset
);