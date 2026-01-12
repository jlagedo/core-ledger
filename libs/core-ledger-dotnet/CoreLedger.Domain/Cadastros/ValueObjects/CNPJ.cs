using System.Text.RegularExpressions;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.ValueObjects;

/// <summary>
///     Value Object representando um CNPJ (Cadastro Nacional da Pessoa Jurídica).
///     Suporta tanto o formato numérico tradicional quanto o novo formato alfanumérico
///     (IN RFB 2.229/2024).
/// </summary>
public sealed partial class CNPJ : IEquatable<CNPJ>
{
    private const int TamanhoCnpjSemDv = 12;
    private const int TamanhoCnpjTotal = 14;
    private const int ValorBase = '0'; // ASCII 48

    // Base: 12 caracteres alfanuméricos (A-Z, 0-9), DV: 2 dígitos numéricos
    private static readonly Regex RegexFormacaoBase = RegexFormacaoBaseCompiled();
    private static readonly Regex RegexFormacaoDv = RegexFormacaoDvCompiled();
    private static readonly Regex RegexCaracteresFormatacao = RegexCaracteresFormatacaoCompiled();
    private static readonly Regex RegexValorZerado = RegexValorZeradoCompiled();

    // Pesos para cálculo dos dígitos verificadores
    private static readonly int[] PesosDv = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    /// <summary>
    ///     CNPJ normalizado (14 caracteres alfanuméricos, uppercase).
    /// </summary>
    public string Valor { get; }

    private CNPJ(string valor)
    {
        Valor = valor;
    }

    /// <summary>
    ///     Cria uma instância de CNPJ a partir de uma string.
    /// </summary>
    /// <param name="cnpj">CNPJ com ou sem formatação.</param>
    /// <returns>Instância de CNPJ validada.</returns>
    /// <exception cref="DomainValidationException">Quando o CNPJ é inválido.</exception>
    public static CNPJ Criar(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            throw new DomainValidationException("CNPJ não pode ser vazio.");

        var cnpjNormalizado = RemoverCaracteresFormatacao(cnpj);

        if (!IsCnpjFormacaoValidaComDv(cnpjNormalizado))
            throw new DomainValidationException("CNPJ deve conter 12 caracteres alfanuméricos + 2 dígitos verificadores.");

        if (RegexValorZerado.IsMatch(cnpjNormalizado[..TamanhoCnpjSemDv]))
            throw new DomainValidationException("CNPJ inválido.");

        var baseCnpj = cnpjNormalizado[..TamanhoCnpjSemDv];
        var dvInformado = cnpjNormalizado[TamanhoCnpjSemDv..];
        var dvCalculado = CalcularDv(baseCnpj);

        if (dvCalculado != dvInformado)
            throw new DomainValidationException("CNPJ inválido - dígitos verificadores incorretos.");

        return new CNPJ(cnpjNormalizado);
    }

    /// <summary>
    ///     Tenta criar uma instância de CNPJ a partir de uma string.
    /// </summary>
    /// <param name="cnpj">CNPJ com ou sem formatação.</param>
    /// <param name="resultado">Instância de CNPJ se válido, null caso contrário.</param>
    /// <returns>True se o CNPJ é válido, false caso contrário.</returns>
    public static bool TentarCriar(string cnpj, out CNPJ? resultado)
    {
        resultado = null;

        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        var cnpjNormalizado = RemoverCaracteresFormatacao(cnpj);

        if (!IsCnpjFormacaoValidaComDv(cnpjNormalizado))
            return false;

        if (RegexValorZerado.IsMatch(cnpjNormalizado[..TamanhoCnpjSemDv]))
            return false;

        var baseCnpj = cnpjNormalizado[..TamanhoCnpjSemDv];
        var dvInformado = cnpjNormalizado[TamanhoCnpjSemDv..];
        var dvCalculado = CalcularDv(baseCnpj);

        if (dvCalculado != dvInformado)
            return false;

        resultado = new CNPJ(cnpjNormalizado);
        return true;
    }

    /// <summary>
    ///     Retorna o CNPJ formatado (XX.XXX.XXX/XXXX-XX).
    /// </summary>
    public string Formatado =>
        $"{Valor[..2]}.{Valor[2..5]}.{Valor[5..8]}/{Valor[8..12]}-{Valor[12..]}";

    /// <summary>
    ///     Indica se o CNPJ é alfanumérico (contém letras na base).
    /// </summary>
    public bool IsAlfanumerico => Valor[..TamanhoCnpjSemDv].Any(char.IsLetter);

    /// <summary>
    ///     Calcula os dígitos verificadores para uma base de CNPJ.
    /// </summary>
    /// <param name="baseCnpj">Base do CNPJ (12 caracteres alfanuméricos).</param>
    /// <returns>String com os 2 dígitos verificadores.</returns>
    public static string CalcularDv(string baseCnpj)
    {
        if (string.IsNullOrWhiteSpace(baseCnpj))
            throw new ArgumentException("Base do CNPJ não pode ser vazia.", nameof(baseCnpj));

        baseCnpj = RemoverCaracteresFormatacao(baseCnpj);

        if (!IsCnpjFormacaoValidaSemDv(baseCnpj))
            throw new ArgumentException($"Base do CNPJ '{baseCnpj}' não é válida para cálculo do DV.", nameof(baseCnpj));

        var dv1 = CalcularDigito(baseCnpj);
        var dv2 = CalcularDigito(baseCnpj + dv1);

        return $"{dv1}{dv2}";
    }

    private static int CalcularDigito(string cnpj)
    {
        var soma = 0;
        var tamanho = cnpj.Length;

        for (var i = tamanho - 1; i >= 0; i--)
        {
            var valorCaracter = cnpj[i] - ValorBase;
            var indicePeso = PesosDv.Length - tamanho + i;
            soma += valorCaracter * PesosDv[indicePeso];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static string RemoverCaracteresFormatacao(string cnpj)
    {
        return RegexCaracteresFormatacao.Replace(cnpj.Trim().ToUpperInvariant(), "");
    }

    private static bool IsCnpjFormacaoValidaSemDv(string cnpj)
    {
        return cnpj.Length == TamanhoCnpjSemDv &&
               RegexFormacaoBase.IsMatch(cnpj) &&
               !RegexValorZerado.IsMatch(cnpj);
    }

    private static bool IsCnpjFormacaoValidaComDv(string cnpj)
    {
        if (cnpj.Length != TamanhoCnpjTotal)
            return false;

        var baseCnpj = cnpj[..TamanhoCnpjSemDv];
        var dv = cnpj[TamanhoCnpjSemDv..];

        return RegexFormacaoBase.IsMatch(baseCnpj) && RegexFormacaoDv.IsMatch(dv);
    }

    [GeneratedRegex(@"^[A-Z\d]{12}$")]
    private static partial Regex RegexFormacaoBaseCompiled();

    [GeneratedRegex(@"^\d{2}$")]
    private static partial Regex RegexFormacaoDvCompiled();

    [GeneratedRegex(@"[./-]")]
    private static partial Regex RegexCaracteresFormatacaoCompiled();

    [GeneratedRegex(@"^0+$")]
    private static partial Regex RegexValorZeradoCompiled();

    public bool Equals(CNPJ? other)
    {
        if (other is null) return false;
        return Valor == other.Valor;
    }

    public override bool Equals(object? obj)
    {
        return obj is CNPJ other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Valor.GetHashCode();
    }

    public override string ToString()
    {
        return Formatado;
    }

    public static bool operator ==(CNPJ? left, CNPJ? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(CNPJ? left, CNPJ? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Conversão implícita de CNPJ para string (retorna valor sem formatação).
    /// </summary>
    public static implicit operator string(CNPJ cnpj) => cnpj.Valor;
}
