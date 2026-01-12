using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
///     Testes unitários para a entidade Instituicao.
/// </summary>
public class InstituicaoTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange
        var cnpj = "11222333000181";
        var razaoSocial = "Banco de Investimentos XYZ S.A.";

        // Act
        var instituicao = Instituicao.Criar(cnpj, razaoSocial);

        // Assert
        Assert.NotNull(instituicao);
        Assert.Equal(cnpj, instituicao.Cnpj.Valor);
        Assert.Equal(razaoSocial, instituicao.RazaoSocial);
        Assert.Null(instituicao.NomeFantasia);
        Assert.True(instituicao.Ativo);
    }

    [Fact]
    public void Criar_ComNomeFantasia_DeveCriarComNomeFantasia()
    {
        // Arrange
        var cnpj = "11222333000181";
        var razaoSocial = "Banco de Investimentos XYZ S.A.";
        var nomeFantasia = "Banco XYZ";

        // Act
        var instituicao = Instituicao.Criar(cnpj, razaoSocial, nomeFantasia);

        // Assert
        Assert.NotNull(instituicao);
        Assert.Equal(nomeFantasia, instituicao.NomeFantasia);
    }

    [Fact]
    public void Criar_ComRazaoSocialVazia_DeveLancarExcecao()
    {
        // Arrange
        var cnpj = "11222333000181";
        var razaoSocial = "";

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            Instituicao.Criar(cnpj, razaoSocial));
    }

    [Fact]
    public void Criar_ComRazaoSocialMuitoLonga_DeveLancarExcecao()
    {
        // Arrange
        var cnpj = "11222333000181";
        var razaoSocial = new string('A', 201);

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            Instituicao.Criar(cnpj, razaoSocial));
    }

    [Fact]
    public void Criar_ComNomeFantasiaMuitoLongo_DeveLancarExcecao()
    {
        // Arrange
        var cnpj = "11222333000181";
        var razaoSocial = "Banco de Investimentos XYZ S.A.";
        var nomeFantasia = new string('A', 101);

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            Instituicao.Criar(cnpj, razaoSocial, nomeFantasia));
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void AtualizarDadosCadastrais_ComDadosValidos_DeveAtualizar()
    {
        // Arrange
        var instituicao = Instituicao.Criar("11222333000181", "Razão Original");
        var novaRazaoSocial = "Nova Razão Social Ltda.";
        var novoNomeFantasia = "Nova Fantasia";

        // Act
        instituicao.AtualizarDadosCadastrais(novaRazaoSocial, novoNomeFantasia);

        // Assert
        Assert.Equal(novaRazaoSocial, instituicao.RazaoSocial);
        Assert.Equal(novoNomeFantasia, instituicao.NomeFantasia);
        Assert.NotNull(instituicao.UpdatedAt);
    }

    [Fact]
    public void AtualizarDadosCadastrais_ComRazaoSocialVazia_DeveLancarExcecao()
    {
        // Arrange
        var instituicao = Instituicao.Criar("11222333000181", "Razão Original");

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            instituicao.AtualizarDadosCadastrais("", null));
    }

    #endregion

    #region Testes de Ativação/Inativação

    [Fact]
    public void Inativar_DeveMarcarComoInativo()
    {
        // Arrange
        var instituicao = Instituicao.Criar("11222333000181", "Banco XYZ S.A.");

        // Act
        instituicao.Inativar();

        // Assert
        Assert.False(instituicao.Ativo);
        Assert.NotNull(instituicao.UpdatedAt);
    }

    [Fact]
    public void Ativar_DeveMarcarComoAtivo()
    {
        // Arrange
        var instituicao = Instituicao.Criar("11222333000181", "Banco XYZ S.A.", ativo: false);

        // Act
        instituicao.Ativar();

        // Assert
        Assert.True(instituicao.Ativo);
        Assert.NotNull(instituicao.UpdatedAt);
    }

    #endregion
}
