using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
/// Testes unitários para a entidade FundoTaxaPerformance.
/// </summary>
public class FundoTaxaPerformanceTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange & Act
        var parametros = FundoTaxaPerformance.Criar(
            fundoTaxaId: 1,
            indexadorId: 1,
            percentualBenchmark: 100m,
            metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
            linhaDagua: true,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral,
            mesCristalizacao: 6);

        // Assert
        Assert.NotNull(parametros);
        Assert.Equal(1, parametros.FundoTaxaId);
        Assert.Equal(1, parametros.IndexadorId);
        Assert.Equal(100m, parametros.PercentualBenchmark);
        Assert.Equal(MetodoCalculoPerformance.HighWaterMark, parametros.MetodoCalculo);
        Assert.True(parametros.LinhaDagua);
        Assert.Equal(PeriodicidadeCristalizacao.Semestral, parametros.PeriodicidadeCristalizacao);
        Assert.Equal(6, parametros.MesCristalizacao);
    }

    [Fact]
    public void CriarSemTaxa_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange & Act
        var parametros = FundoTaxaPerformance.CriarSemTaxa(
            indexadorId: 2,
            percentualBenchmark: 110m,
            metodoCalculo: MetodoCalculoPerformance.CotaAjustada,
            linhaDagua: false,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Anual,
            mesCristalizacao: 12);

        // Assert
        Assert.NotNull(parametros);
        Assert.Equal(0, parametros.FundoTaxaId); // Será definido pelo EF
        Assert.Equal(2, parametros.IndexadorId);
        Assert.Equal(110m, parametros.PercentualBenchmark);
        Assert.Equal(MetodoCalculoPerformance.CotaAjustada, parametros.MetodoCalculo);
        Assert.False(parametros.LinhaDagua);
        Assert.Equal(PeriodicidadeCristalizacao.Anual, parametros.PeriodicidadeCristalizacao);
        Assert.Equal(12, parametros.MesCristalizacao);
    }

    [Fact]
    public void Criar_SemMesCristalizacao_DeveCriarComMesNull()
    {
        // Arrange & Act
        var parametros = FundoTaxaPerformance.Criar(
            fundoTaxaId: 1,
            indexadorId: 1,
            percentualBenchmark: 100m,
            metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
            linhaDagua: true,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral);

        // Assert
        Assert.Null(parametros.MesCristalizacao);
    }

    #endregion

    #region Testes de Validação

    [Fact]
    public void Criar_ComFundoTaxaIdInvalido_DeveLancarExcecao()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxaPerformance.Criar(
                fundoTaxaId: 0,
                indexadorId: 1,
                percentualBenchmark: 100m,
                metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
                linhaDagua: true,
                periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral));

        Assert.Equal("FundoTaxaId é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComIndexadorIdInvalido_DeveLancarExcecao()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxaPerformance.Criar(
                fundoTaxaId: 1,
                indexadorId: 0,
                percentualBenchmark: 100m,
                metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
                linhaDagua: true,
                periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral));

        Assert.Equal("IndexadorId (benchmark) é obrigatório.", exception.Message);
    }

    [Fact]
    public void CriarSemTaxa_ComIndexadorIdInvalido_DeveLancarExcecao()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxaPerformance.CriarSemTaxa(
                indexadorId: 0,
                percentualBenchmark: 100m,
                metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
                linhaDagua: true,
                periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral));

        Assert.Equal("IndexadorId (benchmark) é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComPercentualBenchmarkZero_DeveLancarExcecao()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxaPerformance.Criar(
                fundoTaxaId: 1,
                indexadorId: 1,
                percentualBenchmark: 0,
                metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
                linhaDagua: true,
                periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral));

        Assert.Equal("Percentual do benchmark deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Criar_ComPercentualBenchmarkNegativo_DeveLancarExcecao()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxaPerformance.Criar(
                fundoTaxaId: 1,
                indexadorId: 1,
                percentualBenchmark: -50m,
                metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
                linhaDagua: true,
                periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral));

        Assert.Equal("Percentual do benchmark deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Criar_ComPercentualBenchmarkMaiorQue1000_DeveLancarExcecao()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxaPerformance.Criar(
                fundoTaxaId: 1,
                indexadorId: 1,
                percentualBenchmark: 1001m,
                metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
                linhaDagua: true,
                periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral));

        Assert.Equal("Percentual do benchmark não pode ser maior que 1000%.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Criar_ComMesCristalizacaoInvalido_DeveLancarExcecao(int mesCristalizacao)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxaPerformance.Criar(
                fundoTaxaId: 1,
                indexadorId: 1,
                percentualBenchmark: 100m,
                metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
                linhaDagua: true,
                periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral,
                mesCristalizacao: mesCristalizacao));

        Assert.Equal("Mês de cristalização deve ser entre 1 e 12.", exception.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Criar_ComMesCristalizacaoValido_DeveCriar(int mesCristalizacao)
    {
        // Arrange & Act
        var parametros = FundoTaxaPerformance.Criar(
            fundoTaxaId: 1,
            indexadorId: 1,
            percentualBenchmark: 100m,
            metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
            linhaDagua: true,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral,
            mesCristalizacao: mesCristalizacao);

        // Assert
        Assert.Equal(mesCristalizacao, parametros.MesCristalizacao);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarParametros()
    {
        // Arrange
        var parametros = FundoTaxaPerformance.Criar(
            fundoTaxaId: 1,
            indexadorId: 1,
            percentualBenchmark: 100m,
            metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
            linhaDagua: true,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral,
            mesCristalizacao: 6);

        // Act
        parametros.Atualizar(
            indexadorId: 2,
            percentualBenchmark: 110m,
            metodoCalculo: MetodoCalculoPerformance.CotaAjustada,
            linhaDagua: false,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Anual,
            mesCristalizacao: 12);

        // Assert
        Assert.Equal(2, parametros.IndexadorId);
        Assert.Equal(110m, parametros.PercentualBenchmark);
        Assert.Equal(MetodoCalculoPerformance.CotaAjustada, parametros.MetodoCalculo);
        Assert.False(parametros.LinhaDagua);
        Assert.Equal(PeriodicidadeCristalizacao.Anual, parametros.PeriodicidadeCristalizacao);
        Assert.Equal(12, parametros.MesCristalizacao);
    }

    [Fact]
    public void Atualizar_ComIndexadorInvalido_DeveLancarExcecao()
    {
        // Arrange
        var parametros = FundoTaxaPerformance.Criar(
            fundoTaxaId: 1,
            indexadorId: 1,
            percentualBenchmark: 100m,
            metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
            linhaDagua: true,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            parametros.Atualizar(
                indexadorId: 0,
                percentualBenchmark: 100m,
                metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
                linhaDagua: true,
                periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral,
                mesCristalizacao: null));

        Assert.Equal("IndexadorId (benchmark) é obrigatório.", exception.Message);
    }

    #endregion
}
