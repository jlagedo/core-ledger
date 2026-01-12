using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
///     Testes unitários para a entidade FundoClasse.
/// </summary>
public class FundoClasseTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act
        var classe = FundoClasse.Criar(
            fundoId: fundoId,
            codigoClasse: "SR",
            nomeClasse: "Classe Sênior",
            tipoFundo: TipoFundo.FI);

        // Assert
        Assert.NotNull(classe);
        Assert.NotEqual(Guid.Empty, classe.Id);
        Assert.Equal(fundoId, classe.FundoId);
        Assert.Equal("SR", classe.CodigoClasse);
        Assert.Equal("Classe Sênior", classe.NomeClasse);
        Assert.True(classe.Ativa);
        Assert.Null(classe.TipoClasseFidc);
        Assert.Null(classe.OrdemSubordinacao);
        Assert.False(classe.ResponsabilidadeLimitada);
        Assert.False(classe.SegregacaoPatrimonial);
    }

    [Fact]
    public void Criar_ComFIDC_DeveCriarComTipoClasseEOrdem()
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act
        var classe = FundoClasse.Criar(
            fundoId: fundoId,
            codigoClasse: "MEZ",
            nomeClasse: "Classe Mezanino",
            tipoFundo: TipoFundo.FIDC,
            tipoClasseFidc: TipoClasseFIDC.Mezanino,
            ordemSubordinacao: 2);

        // Assert
        Assert.NotNull(classe);
        Assert.Equal(TipoClasseFIDC.Mezanino, classe.TipoClasseFidc);
        Assert.Equal(2, classe.OrdemSubordinacao);
    }

    [Fact]
    public void Criar_ComDadosOpcionais_DeveCriarComTodosDados()
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act
        var classe = FundoClasse.Criar(
            fundoId: fundoId,
            codigoClasse: "SUB",
            nomeClasse: "Classe Subordinada",
            tipoFundo: TipoFundo.FIDC,
            cnpjClasse: "12345678000190",
            tipoClasseFidc: TipoClasseFIDC.Subordinada,
            ordemSubordinacao: 3,
            rentabilidadeAlvo: 15.5m,
            responsabilidadeLimitada: true,
            segregacaoPatrimonial: true,
            valorMinimoAplicacao: 100000m);

        // Assert
        Assert.Equal("12345678000190", classe.CnpjClasse);
        Assert.Equal(15.5m, classe.RentabilidadeAlvo);
        Assert.True(classe.ResponsabilidadeLimitada);
        Assert.True(classe.SegregacaoPatrimonial);
        Assert.Equal(100000m, classe.ValorMinimoAplicacao);
    }

    #endregion

    #region Testes de Validação

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComCodigoClasseVazio_DeveLancarExcecao(string? codigoClasse)
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, codigoClasse!, "Nome Classe", TipoFundo.FI));
        Assert.Equal("Código da classe é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComCodigoClasseMuitoLongo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var codigoMuitoLongo = new string('A', 11);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, codigoMuitoLongo, "Nome Classe", TipoFundo.FI));
        Assert.Equal("Código da classe deve ter no máximo 10 caracteres.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComNomeClasseVazio_DeveLancarExcecao(string? nomeClasse)
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, "SR", nomeClasse!, TipoFundo.FI));
        Assert.Equal("Nome da classe é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComNomeClasseMuitoLongo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var nomeMuitoLongo = new string('A', 101);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, "SR", nomeMuitoLongo, TipoFundo.FI));
        Assert.Equal("Nome da classe deve ter no máximo 100 caracteres.", exception.Message);
    }

    [Fact]
    public void Criar_FIDCSemTipoClasse_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, "SR", "Classe Sênior", TipoFundo.FIDC, ordemSubordinacao: 1));
        Assert.Equal("Tipo de classe FIDC é obrigatório para fundos FIDC.", exception.Message);
    }

    [Fact]
    public void Criar_FIDCSemOrdemSubordinacao_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, "SR", "Classe Sênior", TipoFundo.FIDC,
                tipoClasseFidc: TipoClasseFIDC.Senior));
        Assert.Equal("Ordem de subordinação é obrigatória para fundos FIDC.", exception.Message);
    }

    [Fact]
    public void Criar_FIDCComOrdemSubordinacaoZero_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, "SR", "Classe Sênior", TipoFundo.FIDC,
                tipoClasseFidc: TipoClasseFIDC.Senior, ordemSubordinacao: 0));
        Assert.Equal("Ordem de subordinação deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Criar_ComRentabilidadeAlvoNegativa_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, "SR", "Classe Sênior", TipoFundo.FI,
                rentabilidadeAlvo: -1m));
        Assert.Equal("Rentabilidade alvo não pode ser negativa.", exception.Message);
    }

    [Fact]
    public void Criar_ComValorMinimoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoClasse.Criar(fundoId, "SR", "Classe Sênior", TipoFundo.FI,
                valorMinimoAplicacao: -1m));
        Assert.Equal("Valor mínimo de aplicação não pode ser negativo.", exception.Message);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarClasse()
    {
        // Arrange
        var classe = FundoClasse.Criar(
            Guid.NewGuid(), "SR", "Classe Sênior Original", TipoFundo.FI);

        // Act
        classe.Atualizar(
            nomeClasse: "Classe Sênior Atualizada",
            cnpjClasse: "12345678000190",
            tipoClasseFidc: null,
            ordemSubordinacao: null,
            rentabilidadeAlvo: 12m,
            responsabilidadeLimitada: true,
            segregacaoPatrimonial: false,
            valorMinimoAplicacao: 50000m,
            tipoFundo: TipoFundo.FI);

        // Assert
        Assert.Equal("Classe Sênior Atualizada", classe.NomeClasse);
        Assert.Equal("12345678000190", classe.CnpjClasse);
        Assert.Equal(12m, classe.RentabilidadeAlvo);
        Assert.True(classe.ResponsabilidadeLimitada);
        Assert.NotNull(classe.UpdatedAt);
    }

    #endregion

    #region Testes de Ativação/Desativação

    [Fact]
    public void Ativar_DeveAtivarClasse()
    {
        // Arrange
        var classe = FundoClasse.Criar(
            Guid.NewGuid(), "SR", "Classe Sênior", TipoFundo.FI);
        classe.Desativar();

        // Act
        classe.Ativar();

        // Assert
        Assert.True(classe.Ativa);
    }

    [Fact]
    public void Desativar_DeveDesativarClasse()
    {
        // Arrange
        var classe = FundoClasse.Criar(
            Guid.NewGuid(), "SR", "Classe Sênior", TipoFundo.FI);

        // Act
        classe.Desativar();

        // Assert
        Assert.False(classe.Ativa);
    }

    #endregion

    #region Testes de Exclusão

    [Fact]
    public void Excluir_SemSubclasses_DeveExcluir()
    {
        // Arrange
        var classe = FundoClasse.Criar(
            Guid.NewGuid(), "SR", "Classe Sênior", TipoFundo.FI);

        // Act
        classe.Excluir();

        // Assert
        Assert.NotNull(classe.DeletedAt);
    }

    [Fact]
    public void Restaurar_DeveRestaurar()
    {
        // Arrange
        var classe = FundoClasse.Criar(
            Guid.NewGuid(), "SR", "Classe Sênior", TipoFundo.FI);
        classe.Excluir();

        // Act
        classe.Restaurar();

        // Assert
        Assert.Null(classe.DeletedAt);
        Assert.NotNull(classe.UpdatedAt);
    }

    #endregion

    #region Testes de Métodos Estáticos

    [Theory]
    [InlineData(TipoFundo.FI, true)]
    [InlineData(TipoFundo.FIC, true)]
    [InlineData(TipoFundo.FIDC, true)]
    [InlineData(TipoFundo.FICFIDC, true)]
    [InlineData(TipoFundo.FIP, false)]
    [InlineData(TipoFundo.FII, false)]
    [InlineData(TipoFundo.FIAGRO, false)]
    public void FundoPermiteClasses_DeveRetornarCorretamente(TipoFundo tipoFundo, bool esperado)
    {
        // Act
        var resultado = FundoClasse.FundoPermiteClasses(tipoFundo);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData(TipoFundo.FIDC, true)]
    [InlineData(TipoFundo.FICFIDC, true)]
    [InlineData(TipoFundo.FI, false)]
    [InlineData(TipoFundo.FIC, false)]
    [InlineData(TipoFundo.FIP, false)]
    public void EhFIDC_DeveRetornarCorretamente(TipoFundo tipoFundo, bool esperado)
    {
        // Act
        var resultado = FundoClasse.EhFIDC(tipoFundo);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    #endregion
}
