using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
/// Testes unitários para a entidade FundoPrazoExcecao.
/// </summary>
public class FundoPrazoExcecaoTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var motivo = "Fechamento de balanço anual";

        // Act
        var excecao = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 3,
            diasLiquidacao: 5,
            motivo: motivo);

        // Assert
        Assert.NotNull(excecao);
        Assert.Equal(prazoId, excecao.PrazoId);
        Assert.Equal(dataInicio, excecao.DataInicio);
        Assert.Equal(dataFim, excecao.DataFim);
        Assert.Equal(3, excecao.DiasCotizacao);
        Assert.Equal(5, excecao.DiasLiquidacao);
        Assert.Equal(motivo, excecao.Motivo);
    }

    [Fact]
    public void Criar_PrazoIdInvalido_DeveLancarExcecao()
    {
        // Arrange
        var prazoId = 0L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Teste"));

        Assert.Equal("PrazoId é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_DataFimAnteriorADataInicio_DeveLancarExcecao()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Teste"));

        Assert.Equal("Data de fim não pode ser anterior à data de início.", exception.Message);
    }

    [Fact]
    public void Criar_DiasCotizacaoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: -1,
            diasLiquidacao: 3,
            motivo: "Teste"));

        Assert.Equal("Dias de cotização não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Criar_DiasLiquidacaoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: -3,
            motivo: "Teste"));

        Assert.Equal("Dias de liquidação não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Criar_MotivoVazio_DeveLancarExcecao()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: ""));

        Assert.Equal("Motivo é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_MotivoMuitoLongo_DeveLancarExcecao()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var motivoLongo = new string('A', 201);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: motivoLongo));

        Assert.Equal("Motivo deve ter no máximo 200 caracteres.", exception.Message);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarExcecao()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicioOriginal = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFimOriginal = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var excecao = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicioOriginal,
            dataFim: dataFimOriginal,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Motivo original");

        var novaDataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var novaDataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(15));
        var novoMotivo = "Novo motivo";

        // Act
        excecao.Atualizar(
            dataInicio: novaDataInicio,
            dataFim: novaDataFim,
            diasCotizacao: 4,
            diasLiquidacao: 6,
            motivo: novoMotivo);

        // Assert
        Assert.Equal(novaDataInicio, excecao.DataInicio);
        Assert.Equal(novaDataFim, excecao.DataFim);
        Assert.Equal(4, excecao.DiasCotizacao);
        Assert.Equal(6, excecao.DiasLiquidacao);
        Assert.Equal(novoMotivo, excecao.Motivo);
    }

    #endregion

    #region Testes de Verificação

    [Fact]
    public void EstaAtivaEm_DataDentroDoPerido_DeveRetornarTrue()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var excecao = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Teste");

        var dataVerificacao = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

        // Act
        var resultado = excecao.EstaAtivaEm(dataVerificacao);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void EstaAtivaEm_DataForaDoPerido_DeveRetornarFalse()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var excecao = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Teste");

        var dataVerificacao = DateOnly.FromDateTime(DateTime.Today.AddDays(15));

        // Act
        var resultado = excecao.EstaAtivaEm(dataVerificacao);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void EstaAtivaEm_DataIgualADataInicio_DeveRetornarTrue()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var excecao = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Teste");

        // Act
        var resultado = excecao.EstaAtivaEm(dataInicio);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void EstaAtivaEm_DataIgualADataFim_DeveRetornarTrue()
    {
        // Arrange
        var prazoId = 1L;
        var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var excecao = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: dataInicio,
            dataFim: dataFim,
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Teste");

        // Act
        var resultado = excecao.EstaAtivaEm(dataFim);

        // Assert
        Assert.True(resultado);
    }

    #endregion
}
