using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
/// Testes unitários para a entidade FundoTaxa.
/// </summary>
public class FundoTaxaTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var taxa = FundoTaxa.Criar(
            fundoId: fundoId,
            tipoTaxa: TipoTaxa.Administracao,
            percentual: 1.5m,
            baseCalculo: BaseCalculoTaxa.PLMedio,
            periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
            periodicidadePagamento: PeriodicidadePagamento.Mensal,
            dataInicioVigencia: dataInicio);

        // Assert
        Assert.NotNull(taxa);
        Assert.Equal(fundoId, taxa.FundoId);
        Assert.Equal(TipoTaxa.Administracao, taxa.TipoTaxa);
        Assert.Equal(1.5m, taxa.Percentual);
        Assert.Equal(BaseCalculoTaxa.PLMedio, taxa.BaseCalculo);
        Assert.Equal(PeriodicidadeProvisao.Diaria, taxa.PeriodicidadeProvisao);
        Assert.Equal(PeriodicidadePagamento.Mensal, taxa.PeriodicidadePagamento);
        Assert.Equal(dataInicio, taxa.DataInicioVigencia);
        Assert.Null(taxa.DataFimVigencia);
        Assert.True(taxa.Ativa);
        Assert.Null(taxa.ClasseId);
        Assert.Null(taxa.DiaPagamento);
        Assert.Null(taxa.ValorMinimo);
        Assert.Null(taxa.ValorMaximo);
    }

    [Fact]
    public void Criar_ComDadosOpcionais_DeveCriarComTodosDados()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var classeId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var taxa = FundoTaxa.Criar(
            fundoId: fundoId,
            tipoTaxa: TipoTaxa.Administracao,
            percentual: 2.0m,
            baseCalculo: BaseCalculoTaxa.PLFinal,
            periodicidadeProvisao: PeriodicidadeProvisao.Mensal,
            periodicidadePagamento: PeriodicidadePagamento.Trimestral,
            dataInicioVigencia: dataInicio,
            classeId: classeId,
            diaPagamento: 15,
            valorMinimo: 1000m,
            valorMaximo: 50000m);

        // Assert
        Assert.Equal(classeId, taxa.ClasseId);
        Assert.Equal(15, taxa.DiaPagamento);
        Assert.Equal(1000m, taxa.ValorMinimo);
        Assert.Equal(50000m, taxa.ValorMaximo);
    }

    [Fact]
    public void Criar_TaxaPerformance_DeveIndicarQueRequerParametros()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var taxa = FundoTaxa.Criar(
            fundoId: fundoId,
            tipoTaxa: TipoTaxa.Performance,
            percentual: 20.0m,
            baseCalculo: BaseCalculoTaxa.Rentabilidade,
            periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
            periodicidadePagamento: PeriodicidadePagamento.Semestral,
            dataInicioVigencia: dataInicio);

        // Assert
        Assert.True(taxa.EhTaxaPerformance);
        Assert.True(taxa.RequerParametrosPerformance);
    }

    #endregion

    #region Testes de Validação

    [Fact]
    public void Criar_ComFundoIdVazio_DeveLancarExcecao()
    {
        // Arrange
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxa.Criar(
                fundoId: Guid.Empty,
                tipoTaxa: TipoTaxa.Administracao,
                percentual: 1.5m,
                baseCalculo: BaseCalculoTaxa.PLMedio,
                periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
                periodicidadePagamento: PeriodicidadePagamento.Mensal,
                dataInicioVigencia: dataInicio));

        Assert.Equal("FundoId é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComPercentualNegativo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxa.Criar(
                fundoId: fundoId,
                tipoTaxa: TipoTaxa.Administracao,
                percentual: -1.0m,
                baseCalculo: BaseCalculoTaxa.PLMedio,
                periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
                periodicidadePagamento: PeriodicidadePagamento.Mensal,
                dataInicioVigencia: dataInicio));

        Assert.Equal("Percentual não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Criar_ComPercentualMaiorQue100_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxa.Criar(
                fundoId: fundoId,
                tipoTaxa: TipoTaxa.Administracao,
                percentual: 101.0m,
                baseCalculo: BaseCalculoTaxa.PLMedio,
                periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
                periodicidadePagamento: PeriodicidadePagamento.Mensal,
                dataInicioVigencia: dataInicio));

        Assert.Equal("Percentual não pode ser maior que 100%.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    [InlineData(31)]
    public void Criar_ComDiaPagamentoInvalido_DeveLancarExcecao(int diaPagamento)
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxa.Criar(
                fundoId: fundoId,
                tipoTaxa: TipoTaxa.Administracao,
                percentual: 1.5m,
                baseCalculo: BaseCalculoTaxa.PLMedio,
                periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
                periodicidadePagamento: PeriodicidadePagamento.Mensal,
                dataInicioVigencia: dataInicio,
                diaPagamento: diaPagamento));

        Assert.Equal("Dia de pagamento deve ser entre 1 e 28.", exception.Message);
    }

    [Fact]
    public void Criar_ComValorMinimoMaiorQueMaximo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxa.Criar(
                fundoId: fundoId,
                tipoTaxa: TipoTaxa.Administracao,
                percentual: 1.5m,
                baseCalculo: BaseCalculoTaxa.PLMedio,
                periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
                periodicidadePagamento: PeriodicidadePagamento.Mensal,
                dataInicioVigencia: dataInicio,
                valorMinimo: 10000m,
                valorMaximo: 5000m));

        Assert.Equal("Valor mínimo não pode ser maior que valor máximo.", exception.Message);
    }

    [Fact]
    public void Criar_ComValorMinimoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoTaxa.Criar(
                fundoId: fundoId,
                tipoTaxa: TipoTaxa.Administracao,
                percentual: 1.5m,
                baseCalculo: BaseCalculoTaxa.PLMedio,
                periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
                periodicidadePagamento: PeriodicidadePagamento.Mensal,
                dataInicioVigencia: dataInicio,
                valorMinimo: -100m));

        Assert.Equal("Valor mínimo não pode ser negativo.", exception.Message);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarTaxa()
    {
        // Arrange
        var taxa = CriarTaxaValida();

        // Act
        taxa.Atualizar(
            percentual: 2.0m,
            baseCalculo: BaseCalculoTaxa.PLFinal,
            periodicidadeProvisao: PeriodicidadeProvisao.Mensal,
            periodicidadePagamento: PeriodicidadePagamento.Trimestral,
            diaPagamento: 20,
            valorMinimo: 500m,
            valorMaximo: 10000m);

        // Assert
        Assert.Equal(2.0m, taxa.Percentual);
        Assert.Equal(BaseCalculoTaxa.PLFinal, taxa.BaseCalculo);
        Assert.Equal(PeriodicidadeProvisao.Mensal, taxa.PeriodicidadeProvisao);
        Assert.Equal(PeriodicidadePagamento.Trimestral, taxa.PeriodicidadePagamento);
        Assert.Equal(20, taxa.DiaPagamento);
        Assert.Equal(500m, taxa.ValorMinimo);
        Assert.Equal(10000m, taxa.ValorMaximo);
        Assert.NotNull(taxa.UpdatedAt);
    }

    #endregion

    #region Testes de Desativação

    [Fact]
    public void Desativar_DevDesativarTaxa()
    {
        // Arrange
        var taxa = CriarTaxaValida();

        // Act
        taxa.Desativar();

        // Assert
        Assert.False(taxa.Ativa);
        Assert.NotNull(taxa.DataFimVigencia);
        Assert.NotNull(taxa.UpdatedAt);
    }

    [Fact]
    public void Desativar_ComDataFimEspecifica_DeveUsarDataInformada()
    {
        // Arrange
        var taxa = CriarTaxaValida();
        var dataFim = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

        // Act
        taxa.Desativar(dataFim);

        // Assert
        Assert.Equal(dataFim, taxa.DataFimVigencia);
    }

    [Fact]
    public void Desativar_ComDataFimAnteriorAInicio_DeveLancarExcecao()
    {
        // Arrange
        var taxa = CriarTaxaValida();
        var dataFimAnterior = taxa.DataInicioVigencia.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            taxa.Desativar(dataFimAnterior));

        Assert.Equal("Data de fim de vigência não pode ser anterior à data de início.", exception.Message);
    }

    [Fact]
    public void Reativar_DeveReativarTaxa()
    {
        // Arrange
        var taxa = CriarTaxaValida();
        taxa.Desativar();

        // Act
        taxa.Reativar();

        // Assert
        Assert.True(taxa.Ativa);
        Assert.Null(taxa.DataFimVigencia);
    }

    #endregion

    #region Testes de Parâmetros de Performance

    [Fact]
    public void DefinirParametrosPerformance_ParaTaxaPerformance_DeveDefinirParametros()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var dataInicio = DateOnly.FromDateTime(DateTime.Today);
        var taxa = FundoTaxa.Criar(
            fundoId: fundoId,
            tipoTaxa: TipoTaxa.Performance,
            percentual: 20.0m,
            baseCalculo: BaseCalculoTaxa.Rentabilidade,
            periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
            periodicidadePagamento: PeriodicidadePagamento.Semestral,
            dataInicioVigencia: dataInicio);

        var parametros = FundoTaxaPerformance.CriarSemTaxa(
            indexadorId: 1,
            percentualBenchmark: 100m,
            metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
            linhaDagua: true,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral);

        // Act
        taxa.DefinirParametrosPerformance(parametros);

        // Assert
        Assert.NotNull(taxa.ParametrosPerformance);
        Assert.False(taxa.RequerParametrosPerformance);
    }

    [Fact]
    public void DefinirParametrosPerformance_ParaTaxaNaoPerformance_DeveLancarExcecao()
    {
        // Arrange
        var taxa = CriarTaxaValida();
        var parametros = FundoTaxaPerformance.CriarSemTaxa(
            indexadorId: 1,
            percentualBenchmark: 100m,
            metodoCalculo: MetodoCalculoPerformance.HighWaterMark,
            linhaDagua: true,
            periodicidadeCristalizacao: PeriodicidadeCristalizacao.Semestral);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            taxa.DefinirParametrosPerformance(parametros));

        Assert.Equal("Parâmetros de performance só podem ser definidos para taxas do tipo Performance.", exception.Message);
    }

    #endregion

    #region Helper Methods

    private static FundoTaxa CriarTaxaValida()
    {
        return FundoTaxa.Criar(
            fundoId: Guid.NewGuid(),
            tipoTaxa: TipoTaxa.Administracao,
            percentual: 1.5m,
            baseCalculo: BaseCalculoTaxa.PLMedio,
            periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
            periodicidadePagamento: PeriodicidadePagamento.Mensal,
            dataInicioVigencia: DateOnly.FromDateTime(DateTime.Today));
    }

    #endregion
}
