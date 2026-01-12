using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.ValueObjects;

/// <summary>
///     Testes unitários para o Value Object CodigoANBIMA.
/// </summary>
public class CodigoANBIMATests
{
    #region Testes de Validação - Códigos Válidos

    [Theory]
    [InlineData("123456")]
    [InlineData("000001")]
    [InlineData("999999")]
    public void Criar_ComCodigoValido_DeveCriarInstancia(string codigo)
    {
        // Act
        var resultado = CodigoANBIMA.Criar(codigo);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(codigo, resultado.Valor);
    }

    [Fact]
    public void Criar_ComCodigoComEspacos_DeveExtrairDigitos()
    {
        // Act
        var resultado = CodigoANBIMA.Criar("  123 456  ");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("123456", resultado.Valor);
    }

    #endregion

    #region Testes de Validação - Códigos Inválidos

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComCodigoVazio_DeveLancarDomainValidationException(string? codigo)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CodigoANBIMA.Criar(codigo!));
        Assert.Equal("Código ANBIMA não pode ser vazio.", exception.Message);
    }

    [Theory]
    [InlineData("12345")] // 5 dígitos
    [InlineData("1234567")] // 7 dígitos
    [InlineData("1234")] // muito curto
    public void Criar_ComQuantidadeDigitosIncorreta_DeveLancarDomainValidationException(string codigo)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CodigoANBIMA.Criar(codigo));
        Assert.Equal("Código ANBIMA deve conter 6 dígitos.", exception.Message);
    }

    [Theory]
    [InlineData("abcdef")] // letras
    [InlineData("12abc6")] // misto
    public void Criar_ComCaracteresNaoNumericos_DeveLancarDomainValidationException(string codigo)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CodigoANBIMA.Criar(codigo));
        Assert.Equal("Código ANBIMA deve conter 6 dígitos.", exception.Message);
    }

    #endregion

    #region Testes de TentarCriar

    [Fact]
    public void TentarCriar_ComCodigoValido_DeveRetornarTrue()
    {
        // Act
        var sucesso = CodigoANBIMA.TentarCriar("123456", out var resultado);

        // Assert
        Assert.True(sucesso);
        Assert.NotNull(resultado);
        Assert.Equal("123456", resultado!.Valor);
    }

    [Fact]
    public void TentarCriar_ComCodigoInvalido_DeveRetornarFalse()
    {
        // Act
        var sucesso = CodigoANBIMA.TentarCriar("12345", out var resultado);

        // Assert
        Assert.False(sucesso);
        Assert.Null(resultado);
    }

    [Fact]
    public void TentarCriar_ComCodigoVazio_DeveRetornarFalse()
    {
        // Act
        var sucesso = CodigoANBIMA.TentarCriar("", out var resultado);

        // Assert
        Assert.False(sucesso);
        Assert.Null(resultado);
    }

    #endregion

    #region Testes de Igualdade

    [Fact]
    public void Equals_ComMesmoValor_DeveRetornarTrue()
    {
        // Arrange
        var codigo1 = CodigoANBIMA.Criar("123456");
        var codigo2 = CodigoANBIMA.Criar("123456");

        // Act & Assert
        Assert.True(codigo1.Equals(codigo2));
        Assert.True(codigo1 == codigo2);
        Assert.Equal(codigo1.GetHashCode(), codigo2.GetHashCode());
    }

    [Fact]
    public void Equals_ComValoresDiferentes_DeveRetornarFalse()
    {
        // Arrange
        var codigo1 = CodigoANBIMA.Criar("123456");
        var codigo2 = CodigoANBIMA.Criar("654321");

        // Act & Assert
        Assert.False(codigo1.Equals(codigo2));
        Assert.True(codigo1 != codigo2);
    }

    [Fact]
    public void Equals_ComNull_DeveRetornarFalse()
    {
        // Arrange
        var codigo = CodigoANBIMA.Criar("123456");

        // Act & Assert
        Assert.False(codigo.Equals(null));
        Assert.False(codigo == null);
        Assert.True(codigo != null);
    }

    #endregion

    #region Testes de Conversão

    [Fact]
    public void ToString_DeveRetornarValor()
    {
        // Arrange
        var codigo = CodigoANBIMA.Criar("123456");

        // Act & Assert
        Assert.Equal("123456", codigo.ToString());
    }

    [Fact]
    public void ConversaoImplicita_DeveRetornarValor()
    {
        // Arrange
        var codigo = CodigoANBIMA.Criar("123456");

        // Act
        string valor = codigo;

        // Assert
        Assert.Equal("123456", valor);
    }

    #endregion
}
