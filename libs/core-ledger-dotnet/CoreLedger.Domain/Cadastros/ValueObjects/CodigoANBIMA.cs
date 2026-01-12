using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.ValueObjects;

/// <summary>
///     Value Object representando um código ANBIMA (6 dígitos numéricos).
/// </summary>
public sealed class CodigoANBIMA : IEquatable<CodigoANBIMA>
{
    /// <summary>
    ///     Código ANBIMA (6 dígitos).
    /// </summary>
    public string Valor { get; }

    private CodigoANBIMA(string valor)
    {
        Valor = valor;
    }

    /// <summary>
    ///     Cria uma instância de CodigoANBIMA a partir de uma string.
    /// </summary>
    /// <param name="codigo">Código ANBIMA com 6 dígitos.</param>
    /// <returns>Instância de CodigoANBIMA validada.</returns>
    /// <exception cref="DomainValidationException">Quando o código é inválido.</exception>
    public static CodigoANBIMA Criar(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainValidationException("Código ANBIMA não pode ser vazio.");

        var apenasDigitos = new string(codigo.Where(char.IsDigit).ToArray());

        if (apenasDigitos.Length != 6)
            throw new DomainValidationException("Código ANBIMA deve conter 6 dígitos.");

        return new CodigoANBIMA(apenasDigitos);
    }

    /// <summary>
    ///     Tenta criar uma instância de CodigoANBIMA a partir de uma string.
    /// </summary>
    /// <param name="codigo">Código ANBIMA.</param>
    /// <param name="resultado">Instância de CodigoANBIMA se válido, null caso contrário.</param>
    /// <returns>True se o código é válido, false caso contrário.</returns>
    public static bool TentarCriar(string codigo, out CodigoANBIMA? resultado)
    {
        resultado = null;

        if (string.IsNullOrWhiteSpace(codigo))
            return false;

        var apenasDigitos = new string(codigo.Where(char.IsDigit).ToArray());

        if (apenasDigitos.Length != 6)
            return false;

        resultado = new CodigoANBIMA(apenasDigitos);
        return true;
    }

    public bool Equals(CodigoANBIMA? other)
    {
        if (other is null) return false;
        return Valor == other.Valor;
    }

    public override bool Equals(object? obj)
    {
        return obj is CodigoANBIMA other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Valor.GetHashCode();
    }

    public override string ToString()
    {
        return Valor;
    }

    public static bool operator ==(CodigoANBIMA? left, CodigoANBIMA? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(CodigoANBIMA? left, CodigoANBIMA? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Conversão implícita de CodigoANBIMA para string.
    /// </summary>
    public static implicit operator string(CodigoANBIMA codigo) => codigo.Valor;
}
