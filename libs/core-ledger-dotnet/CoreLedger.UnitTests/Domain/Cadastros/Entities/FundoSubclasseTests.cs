using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
///     Testes unitários para a entidade FundoSubclasse.
/// </summary>
public class FundoSubclasseTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange
        var classeId = Guid.NewGuid();

        // Act
        var subclasse = FundoSubclasse.Criar(
            classeId: classeId,
            codigoSubclasse: "S1",
            nomeSubclasse: "Subclasse Série 1");

        // Assert
        Assert.NotNull(subclasse);
        Assert.Equal(classeId, subclasse.ClasseId);
        Assert.Equal("S1", subclasse.CodigoSubclasse);
        Assert.Equal("Subclasse Série 1", subclasse.NomeSubclasse);
        Assert.True(subclasse.Ativa);
        Assert.Null(subclasse.Serie);
        Assert.Null(subclasse.ValorMinimoAplicacao);
        Assert.Null(subclasse.TaxaAdministracaoDiferenciada);
    }

    [Fact]
    public void Criar_ComDadosOpcionais_DeveCriarComTodosDados()
    {
        // Arrange
        var classeId = Guid.NewGuid();

        // Act
        var subclasse = FundoSubclasse.Criar(
            classeId: classeId,
            codigoSubclasse: "S2",
            nomeSubclasse: "Subclasse Série 2",
            serie: 2,
            valorMinimoAplicacao: 50000m,
            taxaAdministracaoDiferenciada: 1.5m);

        // Assert
        Assert.Equal(2, subclasse.Serie);
        Assert.Equal(50000m, subclasse.ValorMinimoAplicacao);
        Assert.Equal(1.5m, subclasse.TaxaAdministracaoDiferenciada);
    }

    #endregion

    #region Testes de Validação

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComCodigoSubclasseVazio_DeveLancarExcecao(string? codigoSubclasse)
    {
        // Arrange
        var classeId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoSubclasse.Criar(classeId, codigoSubclasse!, "Nome Subclasse"));
        Assert.Equal("Código da subclasse é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComCodigoSubclasseMuitoLongo_DeveLancarExcecao()
    {
        // Arrange
        var classeId = Guid.NewGuid();
        var codigoMuitoLongo = new string('A', 11);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoSubclasse.Criar(classeId, codigoMuitoLongo, "Nome Subclasse"));
        Assert.Equal("Código da subclasse deve ter no máximo 10 caracteres.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComNomeSubclasseVazio_DeveLancarExcecao(string? nomeSubclasse)
    {
        // Arrange
        var classeId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoSubclasse.Criar(classeId, "S1", nomeSubclasse!));
        Assert.Equal("Nome da subclasse é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComNomeSubclasseMuitoLongo_DeveLancarExcecao()
    {
        // Arrange
        var classeId = Guid.NewGuid();
        var nomeMuitoLongo = new string('A', 101);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoSubclasse.Criar(classeId, "S1", nomeMuitoLongo));
        Assert.Equal("Nome da subclasse deve ter no máximo 100 caracteres.", exception.Message);
    }

    [Fact]
    public void Criar_ComSerieZero_DeveLancarExcecao()
    {
        // Arrange
        var classeId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoSubclasse.Criar(classeId, "S1", "Subclasse", serie: 0));
        Assert.Equal("Série deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Criar_ComSerieNegativa_DeveLancarExcecao()
    {
        // Arrange
        var classeId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoSubclasse.Criar(classeId, "S1", "Subclasse", serie: -1));
        Assert.Equal("Série deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Criar_ComValorMinimoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var classeId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoSubclasse.Criar(classeId, "S1", "Subclasse", valorMinimoAplicacao: -1m));
        Assert.Equal("Valor mínimo de aplicação não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Criar_ComTaxaAdministracaoNegativa_DeveLancarExcecao()
    {
        // Arrange
        var classeId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoSubclasse.Criar(classeId, "S1", "Subclasse", taxaAdministracaoDiferenciada: -1m));
        Assert.Equal("Taxa de administração diferenciada não pode ser negativa.", exception.Message);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarSubclasse()
    {
        // Arrange
        var subclasse = FundoSubclasse.Criar(
            Guid.NewGuid(), "S1", "Subclasse Original");

        // Act
        subclasse.Atualizar(
            nomeSubclasse: "Subclasse Atualizada",
            serie: 5,
            valorMinimoAplicacao: 75000m,
            taxaAdministracaoDiferenciada: 2.0m);

        // Assert
        Assert.Equal("Subclasse Atualizada", subclasse.NomeSubclasse);
        Assert.Equal(5, subclasse.Serie);
        Assert.Equal(75000m, subclasse.ValorMinimoAplicacao);
        Assert.Equal(2.0m, subclasse.TaxaAdministracaoDiferenciada);
    }

    #endregion

    #region Testes de Ativação/Desativação

    [Fact]
    public void Ativar_DeveAtivarSubclasse()
    {
        // Arrange
        var subclasse = FundoSubclasse.Criar(
            Guid.NewGuid(), "S1", "Subclasse");
        subclasse.Desativar();

        // Act
        subclasse.Ativar();

        // Assert
        Assert.True(subclasse.Ativa);
    }

    [Fact]
    public void Desativar_DeveDesativarSubclasse()
    {
        // Arrange
        var subclasse = FundoSubclasse.Criar(
            Guid.NewGuid(), "S1", "Subclasse");

        // Act
        subclasse.Desativar();

        // Assert
        Assert.False(subclasse.Ativa);
    }

    #endregion

    #region Testes de Exclusão

    [Fact]
    public void Excluir_DeveExcluirSubclasse()
    {
        // Arrange
        var subclasse = FundoSubclasse.Criar(
            Guid.NewGuid(), "S1", "Subclasse");

        // Act
        subclasse.Excluir();

        // Assert
        Assert.NotNull(subclasse.DeletedAt);
    }

    [Fact]
    public void Restaurar_DeveRestaurarSubclasse()
    {
        // Arrange
        var subclasse = FundoSubclasse.Criar(
            Guid.NewGuid(), "S1", "Subclasse");
        subclasse.Excluir();

        // Act
        subclasse.Restaurar();

        // Assert
        Assert.Null(subclasse.DeletedAt);
    }

    #endregion
}
