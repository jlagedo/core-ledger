using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
///     Testes unitários para a entidade FundoVinculo.
/// </summary>
public class FundoVinculoTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var instituicaoId = 1;
        var tipoVinculo = TipoVinculoInstitucional.Administrador;
        var dataInicio = new DateOnly(2024, 1, 1);

        // Act
        var vinculo = FundoVinculo.Criar(
            fundoId: fundoId,
            instituicaoId: instituicaoId,
            tipoVinculo: tipoVinculo,
            dataInicio: dataInicio);

        // Assert
        Assert.NotNull(vinculo);
        Assert.Equal(fundoId, vinculo.FundoId);
        Assert.Equal(instituicaoId, vinculo.InstituicaoId);
        Assert.Equal(tipoVinculo, vinculo.TipoVinculo);
        Assert.Equal(dataInicio, vinculo.DataInicio);
        Assert.Null(vinculo.DataFim);
        Assert.False(vinculo.Principal);
        Assert.True(vinculo.EstaVigente());
    }

    [Fact]
    public void Criar_ComDadosOpcionais_DeveCriarComTodosDados()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var instituicaoId = 1;
        var tipoVinculo = TipoVinculoInstitucional.Gestor;
        var dataInicio = new DateOnly(2024, 1, 1);
        var contratoNumero = "CONT-2024-001";
        var observacao = "Vínculo principal do fundo";

        // Act
        var vinculo = FundoVinculo.Criar(
            fundoId: fundoId,
            instituicaoId: instituicaoId,
            tipoVinculo: tipoVinculo,
            dataInicio: dataInicio,
            principal: true,
            contratoNumero: contratoNumero,
            observacao: observacao);

        // Assert
        Assert.NotNull(vinculo);
        Assert.True(vinculo.Principal);
        Assert.Equal(contratoNumero, vinculo.ContratoNumero);
        Assert.Equal(observacao, vinculo.Observacao);
    }

    [Fact]
    public void Criar_ComFundoIdVazio_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.Empty;
        var instituicaoId = 1;
        var tipoVinculo = TipoVinculoInstitucional.Administrador;
        var dataInicio = new DateOnly(2024, 1, 1);

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            FundoVinculo.Criar(fundoId, instituicaoId, tipoVinculo, dataInicio));
    }

    [Fact]
    public void Criar_ComInstituicaoIdInvalido_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var instituicaoId = 0;
        var tipoVinculo = TipoVinculoInstitucional.Administrador;
        var dataInicio = new DateOnly(2024, 1, 1);

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            FundoVinculo.Criar(fundoId, instituicaoId, tipoVinculo, dataInicio));
    }

    [Fact]
    public void Criar_ComContratoNumeroMuitoLongo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var instituicaoId = 1;
        var tipoVinculo = TipoVinculoInstitucional.Administrador;
        var dataInicio = new DateOnly(2024, 1, 1);
        var contratoNumero = new string('A', 51);

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            FundoVinculo.Criar(fundoId, instituicaoId, tipoVinculo, dataInicio, contratoNumero: contratoNumero));
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizar()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 1, 1));
        var novaDataInicio = new DateOnly(2024, 2, 1);
        var novoContrato = "CONT-2024-002";
        var novaObservacao = "Atualizado";

        // Act
        vinculo.Atualizar(novaDataInicio, true, novoContrato, novaObservacao);

        // Assert
        Assert.Equal(novaDataInicio, vinculo.DataInicio);
        Assert.True(vinculo.Principal);
        Assert.Equal(novoContrato, vinculo.ContratoNumero);
        Assert.Equal(novaObservacao, vinculo.Observacao);
        Assert.NotNull(vinculo.UpdatedAt);
    }

    [Fact]
    public void Atualizar_VinculoEncerrado_DeveLancarExcecao()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 1, 1));
        vinculo.Encerrar(new DateOnly(2024, 12, 31));

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            vinculo.Atualizar(new DateOnly(2024, 2, 1), false));
    }

    #endregion

    #region Testes de Encerramento

    [Fact]
    public void Encerrar_ComDataValida_DeveEncerrar()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 1, 1));
        var dataFim = new DateOnly(2024, 12, 31);

        // Act
        vinculo.Encerrar(dataFim);

        // Assert
        Assert.Equal(dataFim, vinculo.DataFim);
        Assert.False(vinculo.EstaVigente());
        Assert.NotNull(vinculo.UpdatedAt);
    }

    [Fact]
    public void Encerrar_VinculoJaEncerrado_DeveLancarExcecao()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 1, 1));
        vinculo.Encerrar(new DateOnly(2024, 12, 31));

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            vinculo.Encerrar(new DateOnly(2024, 12, 31)));
    }

    [Fact]
    public void Encerrar_ComDataAnteriorAoInicio_DeveLancarExcecao()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 6, 1));
        var dataFim = new DateOnly(2024, 1, 1);

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            vinculo.Encerrar(dataFim));
    }

    #endregion

    #region Testes de Principal

    [Fact]
    public void DefinirComoPrincipal_DeveMarcarComoPrincipal()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 1, 1));

        // Act
        vinculo.DefinirComoPrincipal(true);

        // Assert
        Assert.True(vinculo.Principal);
        Assert.NotNull(vinculo.UpdatedAt);
    }

    [Fact]
    public void DefinirComoPrincipal_VinculoEncerrado_DeveLancarExcecao()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 1, 1));
        vinculo.Encerrar(new DateOnly(2024, 12, 31));

        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            vinculo.DefinirComoPrincipal(true));
    }

    #endregion

    #region Testes de EstaVigente

    [Fact]
    public void EstaVigente_VinculoSemDataFim_DeveRetornarTrue()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 1, 1));

        // Act & Assert
        Assert.True(vinculo.EstaVigente());
    }

    [Fact]
    public void EstaVigente_VinculoComDataFim_DeveRetornarFalse()
    {
        // Arrange
        var vinculo = FundoVinculo.Criar(
            Guid.NewGuid(), 1, TipoVinculoInstitucional.Administrador, new DateOnly(2024, 1, 1));
        vinculo.Encerrar(new DateOnly(2024, 12, 31));

        // Act & Assert
        Assert.False(vinculo.EstaVigente());
    }

    #endregion
}
