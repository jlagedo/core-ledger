using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
/// Testes unitários para a entidade FundoPrazo.
/// </summary>
public class FundoPrazoTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var horarioLimite = new TimeOnly(14, 0);

        // Act
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: horarioLimite,
            diasUteis: true);

        // Assert
        Assert.NotNull(prazo);
        Assert.Equal(fundoId, prazo.FundoId);
        Assert.Equal(TipoPrazoOperacional.Aplicacao, prazo.TipoPrazo);
        Assert.Equal(1, prazo.DiasCotizacao);
        Assert.Equal(2, prazo.DiasLiquidacao);
        Assert.Equal(horarioLimite, prazo.HorarioLimite);
        Assert.True(prazo.DiasUteis);
        Assert.True(prazo.Ativo);
        Assert.Null(prazo.ClasseId);
        Assert.Null(prazo.DiasCarencia);
        Assert.Null(prazo.CalendarioId);
        Assert.False(prazo.PermiteParcial);
        Assert.Null(prazo.PercentualMinimo);
        Assert.Null(prazo.ValorMinimo);
    }

    [Fact]
    public void Criar_ComDadosOpcionais_DeveCriarComTodosDados()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var classeId = Guid.NewGuid();
        var horarioLimite = new TimeOnly(15, 0);

        // Act
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Resgate,
            diasCotizacao: 3,
            diasLiquidacao: 5,
            horarioLimite: horarioLimite,
            diasUteis: true,
            classeId: classeId,
            diasCarencia: 30,
            calendarioId: 1,
            permiteParcial: true,
            percentualMinimo: 10.5m,
            valorMinimo: 1000m);

        // Assert
        Assert.Equal(classeId, prazo.ClasseId);
        Assert.Equal(30, prazo.DiasCarencia);
        Assert.Equal(1, prazo.CalendarioId);
        Assert.True(prazo.PermiteParcial);
        Assert.Equal(10.5m, prazo.PercentualMinimo);
        Assert.Equal(1000m, prazo.ValorMinimo);
    }

    [Fact]
    public void Criar_FundoIdVazio_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.Empty;
        var horarioLimite = new TimeOnly(14, 0);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: horarioLimite,
            diasUteis: true));

        Assert.Equal("FundoId é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_DiasCotizacaoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var horarioLimite = new TimeOnly(14, 0);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: -1,
            diasLiquidacao: 2,
            horarioLimite: horarioLimite,
            diasUteis: true));

        Assert.Equal("Dias de cotização não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Criar_DiasLiquidacaoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var horarioLimite = new TimeOnly(14, 0);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: -2,
            horarioLimite: horarioLimite,
            diasUteis: true));

        Assert.Equal("Dias de liquidação não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Criar_DiasCarenciaNegativo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var horarioLimite = new TimeOnly(14, 0);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Resgate,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: horarioLimite,
            diasUteis: true,
            diasCarencia: -30));

        Assert.Equal("Dias de carência não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Criar_PercentualMinimoInvalido_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var horarioLimite = new TimeOnly(14, 0);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Resgate,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: horarioLimite,
            diasUteis: true,
            percentualMinimo: 150m));

        Assert.Equal("Percentual mínimo deve estar entre 0 e 100.", exception.Message);
    }

    [Fact]
    public void Criar_ValorMinimoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var horarioLimite = new TimeOnly(14, 0);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Resgate,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: horarioLimite,
            diasUteis: true,
            valorMinimo: -1000m));

        Assert.Equal("Valor mínimo não pode ser negativo.", exception.Message);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarPrazo()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var horarioLimiteOriginal = new TimeOnly(14, 0);
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: horarioLimiteOriginal,
            diasUteis: true);

        var novoHorarioLimite = new TimeOnly(15, 0);

        // Act
        prazo.Atualizar(
            diasCotizacao: 2,
            diasLiquidacao: 3,
            horarioLimite: novoHorarioLimite,
            diasUteis: false,
            diasCarencia: 60,
            calendarioId: 1,
            permiteParcial: true,
            percentualMinimo: 20m,
            valorMinimo: 5000m);

        // Assert
        Assert.Equal(2, prazo.DiasCotizacao);
        Assert.Equal(3, prazo.DiasLiquidacao);
        Assert.Equal(novoHorarioLimite, prazo.HorarioLimite);
        Assert.False(prazo.DiasUteis);
        Assert.Equal(60, prazo.DiasCarencia);
        Assert.Equal(1, prazo.CalendarioId);
        Assert.True(prazo.PermiteParcial);
        Assert.Equal(20m, prazo.PercentualMinimo);
        Assert.Equal(5000m, prazo.ValorMinimo);
        Assert.NotNull(prazo.UpdatedAt);
    }

    #endregion

    #region Testes de Ativar/Desativar

    [Fact]
    public void Desativar_PrazoAtivo_DeveDesativar()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        // Act
        prazo.Desativar();

        // Assert
        Assert.False(prazo.Ativo);
        Assert.NotNull(prazo.UpdatedAt);
    }

    [Fact]
    public void Ativar_PrazoInativo_DeveAtivar()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        prazo.Desativar();

        // Act
        prazo.Ativar();

        // Assert
        Assert.True(prazo.Ativo);
        Assert.NotNull(prazo.UpdatedAt);
    }

    #endregion

    #region Testes de Exceções

    [Fact]
    public void AdicionarExcecao_ComExcecaoValida_DeveAdicionar()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        // Simula um prazoId válido (em um cenário real, seria gerado pelo banco)
        var prazoId = 1L;
        var excecao = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Fechamento de balanço");

        // Act
        prazo.AdicionarExcecao(excecao);

        // Assert
        Assert.Single(prazo.Excecoes);
        Assert.Contains(excecao, prazo.Excecoes);
        Assert.NotNull(prazo.UpdatedAt);
    }

    [Fact]
    public void AdicionarExcecao_ExcecaoNula_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => prazo.AdicionarExcecao(null!));
        Assert.Equal("Exceção não pode ser nula.", exception.Message);
    }

    [Fact]
    public void AdicionarExcecao_ComSobreposicaoInicio_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        var prazoId = 1L;
        // Adiciona primeira exceção: período de 1 a 10 dias
        var excecao1 = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Primeira exceção");

        prazo.AdicionarExcecao(excecao1);

        // Tenta adicionar segunda exceção que sobrepõe no início: período de 5 a 15 dias
        var excecao2 = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(15)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Segunda exceção sobreposta");

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => prazo.AdicionarExcecao(excecao2));
        Assert.Equal("O período da exceção se sobrepõe a uma exceção existente.", exception.Message);
    }

    [Fact]
    public void AdicionarExcecao_ComSobreposicaoFim_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        var prazoId = 1L;
        // Adiciona primeira exceção: período de 10 a 20 dias
        var excecao1 = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Primeira exceção");

        prazo.AdicionarExcecao(excecao1);

        // Tenta adicionar segunda exceção que sobrepõe no fim: período de 5 a 15 dias
        var excecao2 = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(15)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Segunda exceção sobreposta");

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => prazo.AdicionarExcecao(excecao2));
        Assert.Equal("O período da exceção se sobrepõe a uma exceção existente.", exception.Message);
    }

    [Fact]
    public void AdicionarExcecao_EnglobaPeriodoExistente_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        var prazoId = 1L;
        // Adiciona primeira exceção: período de 10 a 15 dias
        var excecao1 = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(15)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Primeira exceção");

        prazo.AdicionarExcecao(excecao1);

        // Tenta adicionar segunda exceção que engloba a primeira: período de 5 a 20 dias
        var excecao2 = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Segunda exceção que engloba");

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => prazo.AdicionarExcecao(excecao2));
        Assert.Equal("O período da exceção se sobrepõe a uma exceção existente.", exception.Message);
    }

    [Fact]
    public void AdicionarExcecao_DentroDePeriodoExistente_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        var prazoId = 1L;
        // Adiciona primeira exceção: período de 5 a 20 dias
        var excecao1 = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Primeira exceção");

        prazo.AdicionarExcecao(excecao1);

        // Tenta adicionar segunda exceção totalmente dentro da primeira: período de 10 a 15 dias
        var excecao2 = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(15)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Segunda exceção dentro da primeira");

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => prazo.AdicionarExcecao(excecao2));
        Assert.Equal("O período da exceção se sobrepõe a uma exceção existente.", exception.Message);
    }

    [Fact]
    public void RemoverExcecao_ExcecaoExistente_DeveRemover()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var prazo = FundoPrazo.Criar(
            fundoId: fundoId,
            tipoPrazo: TipoPrazoOperacional.Aplicacao,
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true);

        // Simula um prazoId válido (em um cenário real, seria gerado pelo banco)
        var prazoId = 1L;
        var excecao = FundoPrazoExcecao.Criar(
            prazoId: prazoId,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
            diasCotizacao: 2,
            diasLiquidacao: 3,
            motivo: "Fechamento de balanço");

        prazo.AdicionarExcecao(excecao);

        // Act
        prazo.RemoverExcecao(excecao.Id);

        // Assert
        Assert.Empty(prazo.Excecoes);
        Assert.NotNull(prazo.UpdatedAt);
    }

    #endregion
}
