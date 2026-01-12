using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.ValueObjects;

/// <summary>
///     Testes unitários para o Value Object CNPJ.
///     Cobre tanto o formato numérico tradicional quanto o alfanumérico (IN RFB 2.229/2024).
/// </summary>
public class CNPJTests
{
    #region Testes de Validação - CNPJs Numéricos Válidos

    [Theory]
    [InlineData("11222333000181")] // CNPJ válido sem formatação
    [InlineData("11.222.333/0001-81")] // CNPJ válido com formatação
    [InlineData("  11.222.333/0001-81  ")] // CNPJ válido com espaços
    public void Criar_ComCNPJNumericoValido_DeveCriarInstancia(string cnpj)
    {
        // Act
        var resultado = CNPJ.Criar(cnpj);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("11222333000181", resultado.Valor);
        Assert.False(resultado.IsAlfanumerico);
    }

    [Theory]
    [InlineData("11222333000181", "11.222.333/0001-81")]
    [InlineData("00000000000191", "00.000.000/0001-91")] // CNPJ válido (Banco do Brasil)
    public void Formatado_DeveRetornarCNPJNumericoFormatado(string entrada, string esperado)
    {
        // Arrange
        var cnpj = CNPJ.Criar(entrada);

        // Act & Assert
        Assert.Equal(esperado, cnpj.Formatado);
    }

    #endregion

    #region Testes de Validação - CNPJs Alfanuméricos Válidos

    [Theory]
    [InlineData("12ABC34501DE35")] // CNPJ alfanumérico válido (base com letras)
    [InlineData("12.ABC.345/01DE-35")] // CNPJ alfanumérico com formatação
    [InlineData("12abc34501de35")] // CNPJ alfanumérico em minúsculas (deve normalizar)
    [InlineData("  12.ABC.345/01DE-35  ")] // CNPJ alfanumérico com espaços
    public void Criar_ComCNPJAlfanumericoValido_DeveCriarInstancia(string cnpj)
    {
        // Act
        var resultado = CNPJ.Criar(cnpj);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("12ABC34501DE35", resultado.Valor);
        Assert.True(resultado.IsAlfanumerico);
    }

    [Theory]
    [InlineData("12ABC34501DE35", "12.ABC.345/01DE-35")]
    [InlineData("AAAAAAAAAA0108", "AA.AAA.AAA/AA01-08")] // CNPJ alfanumérico com muitas letras
    public void Formatado_DeveRetornarCNPJAlfanumericoFormatado(string entrada, string esperado)
    {
        // Arrange
        var cnpj = CNPJ.Criar(entrada);

        // Act & Assert
        Assert.Equal(esperado, cnpj.Formatado);
    }

    [Fact]
    public void IsAlfanumerico_ComCNPJComLetras_DeveRetornarTrue()
    {
        // Arrange
        var cnpj = CNPJ.Criar("12ABC34501DE35");

        // Assert
        Assert.True(cnpj.IsAlfanumerico);
    }

    [Fact]
    public void IsAlfanumerico_ComCNPJApenasDigitos_DeveRetornarFalse()
    {
        // Arrange
        var cnpj = CNPJ.Criar("11222333000181");

        // Assert
        Assert.False(cnpj.IsAlfanumerico);
    }

    #endregion

    #region Testes de Cálculo de DV

    [Theory]
    [InlineData("112223330001", "81")] // Base numérica
    [InlineData("12ABC34501DE", "35")] // Base alfanumérica
    [InlineData("AAAAAAAAAA01", "08")] // Base com muitas letras
    public void CalcularDv_ComBaseValida_DeveRetornarDvCorreto(string baseCnpj, string dvEsperado)
    {
        // Act
        var dv = CNPJ.CalcularDv(baseCnpj);

        // Assert
        Assert.Equal(dvEsperado, dv);
    }

    [Fact]
    public void CalcularDv_ComBaseVazia_DeveLancarArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CNPJ.CalcularDv(""));
    }

    [Fact]
    public void CalcularDv_ComBaseInvalida_DeveLancarArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CNPJ.CalcularDv("12345")); // muito curto
    }

    #endregion

    #region Testes de Validação - CNPJs Inválidos

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComCNPJVazio_DeveLancarDomainValidationException(string? cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj!));
        Assert.Equal("CNPJ não pode ser vazio.", exception.Message);
    }

    [Theory]
    [InlineData("1122233300018")] // 13 caracteres
    [InlineData("112223330001811")] // 15 caracteres
    [InlineData("123456")] // muito curto
    [InlineData("12ABC34501D35")] // 13 caracteres alfanuméricos
    public void Criar_ComQuantidadeCaracteresIncorreta_DeveLancarDomainValidationException(string cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj));
        Assert.Equal("CNPJ deve conter 12 caracteres alfanuméricos + 2 dígitos verificadores.", exception.Message);
    }

    [Theory]
    [InlineData("00000000000000")] // Zeros na base (DVs seriam inválidos de qualquer forma)
    public void Criar_ComBaseZerada_DeveLancarDomainValidationException(string cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj));
        Assert.Equal("CNPJ inválido.", exception.Message);
    }

    [Theory]
    [InlineData("11222333000182")] // dígito verificador errado
    [InlineData("11222333000191")] // dígito verificador errado
    [InlineData("12345678000199")] // dígito verificador errado
    [InlineData("12ABC34501DE36")] // DV errado para CNPJ alfanumérico
    public void Criar_ComDigitosVerificadoresIncorretos_DeveLancarDomainValidationException(string cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj));
        Assert.Equal("CNPJ inválido - dígitos verificadores incorretos.", exception.Message);
    }

    [Theory]
    [InlineData("12ABC34501DEAB")] // DV com letras (inválido - DVs devem ser numéricos)
    [InlineData("12ABC34501DE3A")] // DV parcialmente com letra
    public void Criar_ComDVNaoNumerico_DeveLancarDomainValidationException(string cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj));
        Assert.Equal("CNPJ deve conter 12 caracteres alfanuméricos + 2 dígitos verificadores.", exception.Message);
    }

    [Theory]
    [InlineData("12ABC345@1DE35")] // caractere especial na base
    [InlineData("12ABC345#1DE35")] // caractere especial na base
    public void Criar_ComCaracteresEspeciais_DeveLancarDomainValidationException(string cnpj)
    {
        // Act & Assert
        Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj));
    }

    #endregion

    #region Testes de TentarCriar

    [Fact]
    public void TentarCriar_ComCNPJNumericoValido_DeveRetornarTrue()
    {
        // Act
        var sucesso = CNPJ.TentarCriar("11.222.333/0001-81", out var resultado);

        // Assert
        Assert.True(sucesso);
        Assert.NotNull(resultado);
        Assert.Equal("11222333000181", resultado!.Valor);
    }

    [Fact]
    public void TentarCriar_ComCNPJAlfanumericoValido_DeveRetornarTrue()
    {
        // Act
        var sucesso = CNPJ.TentarCriar("12.ABC.345/01DE-35", out var resultado);

        // Assert
        Assert.True(sucesso);
        Assert.NotNull(resultado);
        Assert.Equal("12ABC34501DE35", resultado!.Valor);
        Assert.True(resultado.IsAlfanumerico);
    }

    [Fact]
    public void TentarCriar_ComCNPJInvalido_DeveRetornarFalse()
    {
        // Act
        var sucesso = CNPJ.TentarCriar("11222333000182", out var resultado);

        // Assert
        Assert.False(sucesso);
        Assert.Null(resultado);
    }

    [Fact]
    public void TentarCriar_ComCNPJVazio_DeveRetornarFalse()
    {
        // Act
        var sucesso = CNPJ.TentarCriar("", out var resultado);

        // Assert
        Assert.False(sucesso);
        Assert.Null(resultado);
    }

    [Fact]
    public void TentarCriar_ComCNPJAlfanumericoInvalido_DeveRetornarFalse()
    {
        // Act
        var sucesso = CNPJ.TentarCriar("12ABC34501DE36", out var resultado); // DV errado

        // Assert
        Assert.False(sucesso);
        Assert.Null(resultado);
    }

    #endregion

    #region Testes de Igualdade

    [Fact]
    public void Equals_ComMesmoValorNumerico_DeveRetornarTrue()
    {
        // Arrange
        var cnpj1 = CNPJ.Criar("11.222.333/0001-81");
        var cnpj2 = CNPJ.Criar("11222333000181");

        // Act & Assert
        Assert.True(cnpj1.Equals(cnpj2));
        Assert.True(cnpj1 == cnpj2);
        Assert.Equal(cnpj1.GetHashCode(), cnpj2.GetHashCode());
    }

    [Fact]
    public void Equals_ComMesmoValorAlfanumerico_DeveRetornarTrue()
    {
        // Arrange
        var cnpj1 = CNPJ.Criar("12.ABC.345/01DE-35");
        var cnpj2 = CNPJ.Criar("12abc34501de35"); // minúsculas devem ser normalizadas

        // Act & Assert
        Assert.True(cnpj1.Equals(cnpj2));
        Assert.True(cnpj1 == cnpj2);
        Assert.Equal(cnpj1.GetHashCode(), cnpj2.GetHashCode());
    }

    [Fact]
    public void Equals_ComValoresDiferentes_DeveRetornarFalse()
    {
        // Arrange
        var cnpj1 = CNPJ.Criar("11222333000181");
        var cnpj2 = CNPJ.Criar("00000000000191");

        // Act & Assert
        Assert.False(cnpj1.Equals(cnpj2));
        Assert.True(cnpj1 != cnpj2);
    }

    [Fact]
    public void Equals_NumericoVsAlfanumerico_DeveRetornarFalse()
    {
        // Arrange
        var cnpjNumerico = CNPJ.Criar("11222333000181");
        var cnpjAlfanumerico = CNPJ.Criar("12ABC34501DE35");

        // Act & Assert
        Assert.False(cnpjNumerico.Equals(cnpjAlfanumerico));
        Assert.True(cnpjNumerico != cnpjAlfanumerico);
    }

    [Fact]
    public void Equals_ComNull_DeveRetornarFalse()
    {
        // Arrange
        var cnpj = CNPJ.Criar("11222333000181");

        // Act & Assert
        Assert.False(cnpj.Equals(null));
        Assert.False(cnpj == null);
        Assert.True(cnpj != null);
    }

    #endregion

    #region Testes de Conversão

    [Fact]
    public void ToString_DeveRetornarFormatadoNumerico()
    {
        // Arrange
        var cnpj = CNPJ.Criar("11222333000181");

        // Act & Assert
        Assert.Equal("11.222.333/0001-81", cnpj.ToString());
    }

    [Fact]
    public void ToString_DeveRetornarFormatadoAlfanumerico()
    {
        // Arrange
        var cnpj = CNPJ.Criar("12ABC34501DE35");

        // Act & Assert
        Assert.Equal("12.ABC.345/01DE-35", cnpj.ToString());
    }

    [Fact]
    public void ConversaoImplicita_DeveRetornarValorSemFormatacao()
    {
        // Arrange
        var cnpj = CNPJ.Criar("11.222.333/0001-81");

        // Act
        string valor = cnpj;

        // Assert
        Assert.Equal("11222333000181", valor);
    }

    [Fact]
    public void ConversaoImplicita_AlfanumericoDeveRetornarUppercase()
    {
        // Arrange
        var cnpj = CNPJ.Criar("12abc34501de35");

        // Act
        string valor = cnpj;

        // Assert
        Assert.Equal("12ABC34501DE35", valor);
    }

    #endregion

    #region Testes de Normalização

    [Fact]
    public void Criar_ComMinusculas_DeveNormalizarParaUppercase()
    {
        // Act
        var cnpj = CNPJ.Criar("12abc34501de35");

        // Assert
        Assert.Equal("12ABC34501DE35", cnpj.Valor);
    }

    [Fact]
    public void Criar_ComMixedCase_DeveNormalizarParaUppercase()
    {
        // Act
        var cnpj = CNPJ.Criar("12AbC34501dE35");

        // Assert
        Assert.Equal("12ABC34501DE35", cnpj.Valor);
    }

    #endregion
}
