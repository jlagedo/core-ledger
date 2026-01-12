using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Cadastros.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CoreLedger.UnitTests.Domain.Cadastros.Services;

/// <summary>
///     Testes para FundoDomainService.
/// </summary>
public class FundoDomainServiceTests
{
    private readonly FundoDomainService _service;
    private readonly ILogger<FundoDomainService> _logger;

    public FundoDomainServiceTests()
    {
        _logger = Substitute.For<ILogger<FundoDomainService>>();
        _service = new FundoDomainService(_logger);
    }

    #region PodeAtivar Tests

    [Fact]
    public void PodeAtivar_FundoEmConstituicaoComTodosRequisitos_DeveRetornarTrue()
    {
        // Arrange
        var fundo = CriarFundoEmConstituicao();
        var classes = CriarClassesAtivas(1);
        var taxas = CriarTaxasObrigatorias();
        var prazos = CriarPrazosObrigatorios();
        var vinculos = CriarVinculosObrigatorios();

        // Act
        var resultado = _service.PodeAtivar(fundo, classes, taxas, prazos, vinculos);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void PodeAtivar_FundoSemClasses_DeveRetornarFalse()
    {
        // Arrange
        var fundo = CriarFundoEmConstituicao();
        var classes = Array.Empty<FundoClasse>();
        var taxas = CriarTaxasObrigatorias();
        var prazos = CriarPrazosObrigatorios();
        var vinculos = CriarVinculosObrigatorios();

        // Act
        var resultado = _service.PodeAtivar(fundo, classes, taxas, prazos, vinculos);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void PodeAtivar_FundoSemTaxaAdministracao_DeveRetornarFalse()
    {
        // Arrange
        var fundo = CriarFundoEmConstituicao();
        var classes = CriarClassesAtivas(1);
        var taxas = Array.Empty<FundoTaxa>(); // Sem taxas
        var prazos = CriarPrazosObrigatorios();
        var vinculos = CriarVinculosObrigatorios();

        // Act
        var resultado = _service.PodeAtivar(fundo, classes, taxas, prazos, vinculos);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void PodeAtivar_FundoSemPrazoAplicacao_DeveRetornarFalse()
    {
        // Arrange
        var fundo = CriarFundoEmConstituicao();
        var classes = CriarClassesAtivas(1);
        var taxas = CriarTaxasObrigatorias();
        var prazos = new[] { CriarPrazo(TipoPrazoOperacional.Resgate) }; // Só resgate
        var vinculos = CriarVinculosObrigatorios();

        // Act
        var resultado = _service.PodeAtivar(fundo, classes, taxas, prazos, vinculos);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void PodeAtivar_FundoSemVinculoAdministrador_DeveRetornarFalse()
    {
        // Arrange
        var fundo = CriarFundoEmConstituicao();
        var classes = CriarClassesAtivas(1);
        var taxas = CriarTaxasObrigatorias();
        var prazos = CriarPrazosObrigatorios();
        // Vinculos sem Administrador
        var vinculos = new[]
        {
            CriarVinculo(TipoVinculoInstitucional.Gestor),
            CriarVinculo(TipoVinculoInstitucional.Custodiante)
        };

        // Act
        var resultado = _service.PodeAtivar(fundo, classes, taxas, prazos, vinculos);

        // Assert
        Assert.False(resultado);
    }

    #endregion

    #region PodeLiquidar Tests

    [Fact]
    public void PodeLiquidar_FundoAtivo_DeveRetornarTrue()
    {
        // Arrange
        var fundo = CriarFundoEmConstituicao();
        fundo.AlterarSituacao(SituacaoFundo.Ativo);

        // Act
        var resultado = _service.PodeLiquidar(fundo);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void PodeLiquidar_FundoSuspenso_DeveRetornarTrue()
    {
        // Arrange
        var fundo = CriarFundoEmConstituicao();
        fundo.AlterarSituacao(SituacaoFundo.Ativo);
        fundo.AlterarSituacao(SituacaoFundo.Suspenso);

        // Act
        var resultado = _service.PodeLiquidar(fundo);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void PodeLiquidar_FundoEmConstituicao_DeveRetornarFalse()
    {
        // Arrange
        var fundo = CriarFundoEmConstituicao();

        // Act
        var resultado = _service.PodeLiquidar(fundo);

        // Assert
        Assert.False(resultado);
    }

    #endregion

    #region CalcularProgressoCadastro Tests

    [Fact]
    public void CalcularProgressoCadastro_FundoCompleto_DeveRetornar95Ou100()
    {
        // Arrange
        var fundo = CriarFundoCompleto();
        var classes = CriarClassesAtivas(1);
        var taxas = CriarTaxasObrigatorias();
        var prazos = CriarPrazosObrigatorios();
        var vinculos = CriarVinculosObrigatorios();

        // Act
        var progresso = _service.CalcularProgressoCadastro(fundo, classes, taxas, prazos, vinculos);

        // Assert - 95% (sem documentos) ou 100%
        Assert.InRange(progresso, 95, 100);
    }

    [Fact]
    public void CalcularProgressoCadastro_FundoSemClasses_DeveSerMenorQue100()
    {
        // Arrange
        var fundo = CriarFundoCompleto();
        var classes = Array.Empty<FundoClasse>();
        var taxas = CriarTaxasObrigatorias();
        var prazos = CriarPrazosObrigatorios();
        var vinculos = CriarVinculosObrigatorios();

        // Act
        var progresso = _service.CalcularProgressoCadastro(fundo, classes, taxas, prazos, vinculos);

        // Assert
        Assert.True(progresso < 90);
    }

    [Fact]
    public void CalcularProgressoCadastro_FundoSemVinculos_DeveRetornarProgressoParcial()
    {
        // Arrange
        var fundo = CriarFundoCompleto();
        var classes = CriarClassesAtivas(1);
        var taxas = CriarTaxasObrigatorias();
        var prazos = CriarPrazosObrigatorios();
        var vinculos = Array.Empty<FundoVinculo>();

        // Act
        var progresso = _service.CalcularProgressoCadastro(fundo, classes, taxas, prazos, vinculos);

        // Assert
        Assert.True(progresso < 80);
    }

    #endregion

    #region ValidarVinculosObrigatorios Tests

    [Fact]
    public void ValidarVinculosObrigatorios_TodosVinculosPresentes_DeveRetornarValido()
    {
        // Arrange
        var vinculos = CriarVinculosObrigatorios();

        // Act
        var resultado = _service.ValidarVinculosObrigatorios(vinculos);

        // Assert
        Assert.True(resultado.IsValid);
        Assert.Empty(resultado.VinculosFaltantes);
    }

    [Fact]
    public void ValidarVinculosObrigatorios_SemAdministrador_DeveRetornarInvalido()
    {
        // Arrange
        var vinculos = new[]
        {
            CriarVinculo(TipoVinculoInstitucional.Gestor),
            CriarVinculo(TipoVinculoInstitucional.Custodiante)
        };

        // Act
        var resultado = _service.ValidarVinculosObrigatorios(vinculos);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(TipoVinculoInstitucional.Administrador, resultado.VinculosFaltantes);
    }

    [Fact]
    public void ValidarVinculosObrigatorios_SemNenhumVinculo_DeveRetornarTodosFaltantes()
    {
        // Arrange
        var vinculos = Array.Empty<FundoVinculo>();

        // Act
        var resultado = _service.ValidarVinculosObrigatorios(vinculos);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Equal(3, resultado.VinculosFaltantes.Count);
    }

    #endregion

    #region PodeAdicionarTaxa Tests

    [Fact]
    public void PodeAdicionarTaxa_SemTaxaDoTipo_DeveRetornarTrue()
    {
        // Arrange
        var taxasExistentes = Array.Empty<FundoTaxa>();

        // Act
        var resultado = _service.PodeAdicionarTaxa(TipoTaxa.Administracao, taxasExistentes);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void PodeAdicionarTaxa_ComTaxaAtivaDoMesmoTipo_DeveRetornarFalse()
    {
        // Arrange
        var taxasExistentes = CriarTaxasObrigatorias();

        // Act
        var resultado = _service.PodeAdicionarTaxa(TipoTaxa.Administracao, taxasExistentes);

        // Assert
        Assert.False(resultado);
    }

    #endregion

    #region Helper Methods

    private static Fundo CriarFundoEmConstituicao()
    {
        return Fundo.Criar(
            cnpj: "11222333000181",
            razaoSocial: "Fundo Teste FI",
            tipoFundo: TipoFundo.FI,
            classificacaoCVM: ClassificacaoCVM.RendaFixa,
            prazo: PrazoFundo.Indeterminado,
            publicoAlvo: PublicoAlvo.Geral,
            tributacao: TributacaoFundo.LongoPrazo,
            condominio: TipoCondominio.Aberto
        );
    }

    private static Fundo CriarFundoCompleto()
    {
        return Fundo.Criar(
            cnpj: "11222333000181",
            razaoSocial: "Fundo Teste FI",
            tipoFundo: TipoFundo.FI,
            classificacaoCVM: ClassificacaoCVM.RendaFixa,
            prazo: PrazoFundo.Indeterminado,
            publicoAlvo: PublicoAlvo.Geral,
            tributacao: TributacaoFundo.LongoPrazo,
            condominio: TipoCondominio.Aberto,
            dataConstituicao: DateOnly.FromDateTime(DateTime.Today.AddYears(-1))
        );
    }

    private static FundoClasse[] CriarClassesAtivas(int quantidade)
    {
        var fundoId = Guid.NewGuid();
        var classes = new FundoClasse[quantidade];
        for (var i = 0; i < quantidade; i++)
        {
            classes[i] = FundoClasse.Criar(
                fundoId: fundoId,
                codigoClasse: $"CLASSE{i + 1}",
                nomeClasse: $"Classe {i + 1}",
                tipoFundo: TipoFundo.FI
            );
        }

        return classes;
    }

    private static FundoTaxa[] CriarTaxasObrigatorias()
    {
        var fundoId = Guid.NewGuid();
        return
        [
            FundoTaxa.Criar(
                fundoId: fundoId,
                tipoTaxa: TipoTaxa.Administracao,
                percentual: 1.5m,
                baseCalculo: BaseCalculoTaxa.PLMedio,
                periodicidadeProvisao: PeriodicidadeProvisao.Diaria,
                periodicidadePagamento: PeriodicidadePagamento.Mensal,
                dataInicioVigencia: DateOnly.FromDateTime(DateTime.Today.AddMonths(-1))
            )
        ];
    }

    private static FundoPrazo[] CriarPrazosObrigatorios()
    {
        var fundoId = Guid.NewGuid();
        return
        [
            CriarPrazo(TipoPrazoOperacional.Aplicacao),
            CriarPrazo(TipoPrazoOperacional.Resgate)
        ];
    }

    private static FundoPrazo CriarPrazo(TipoPrazoOperacional tipoPrazo)
    {
        return FundoPrazo.Criar(
            fundoId: Guid.NewGuid(),
            tipoPrazo: tipoPrazo,
            diasCotizacao: 0,
            diasLiquidacao: 1,
            horarioLimite: new TimeOnly(14, 0),
            diasUteis: true
        );
    }

    private static FundoVinculo[] CriarVinculosObrigatorios()
    {
        return
        [
            CriarVinculo(TipoVinculoInstitucional.Administrador),
            CriarVinculo(TipoVinculoInstitucional.Gestor),
            CriarVinculo(TipoVinculoInstitucional.Custodiante)
        ];
    }

    private static FundoVinculo CriarVinculo(TipoVinculoInstitucional tipoVinculo)
    {
        return FundoVinculo.Criar(
            fundoId: Guid.NewGuid(),
            instituicaoId: 1,
            tipoVinculo: tipoVinculo,
            dataInicio: DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            principal: true
        );
    }

    #endregion
}
