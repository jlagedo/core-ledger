using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.Entities;

/// <summary>
///     Testes unitários para a entidade FundoParametrosFIDC.
/// </summary>
public class FundoParametrosFIDCTests
{
    #region Testes de Criação

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarInstancia()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata, TipoRecebiveis.CCB };

        // Act
        var parametros = FundoParametrosFIDC.Criar(
            fundoId: fundoId,
            tipoFidc: tipoFidc,
            tiposRecebiveis: tiposRecebiveis);

        // Assert
        Assert.NotNull(parametros);
        Assert.Equal(fundoId, parametros.FundoId);
        Assert.Equal(tipoFidc, parametros.TipoFidc);
        Assert.Equal(2, parametros.TiposRecebiveis.Count);
        Assert.Contains(TipoRecebiveis.Duplicata, parametros.TiposRecebiveis);
        Assert.Contains(TipoRecebiveis.CCB, parametros.TiposRecebiveis);
        Assert.False(parametros.PossuiCoobrigacao);
        Assert.False(parametros.IntegracaoRegistradora);
    }

    [Fact]
    public void Criar_ComTodosDadosOpcionais_DeveCriarComTodosDados()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.NaoPadronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.CreditoConsignado };
        var prazoMedioCarteira = 180;
        var indiceSubordinacaoAlvo = 0.15m;
        var indiceSubordinacaoMinimo = 0.10m;
        var provisaoDevedoresDuvidosos = 0.05m;
        var limiteConcentracaoCedente = 0.20m;
        var limiteConcentracaoSacado = 0.25m;
        var percentualCoobrigacao = 0.50m;
        var registradora = Registradora.CERC;
        var codigoRegistradora = "FIDC123456";

        // Act
        var parametros = FundoParametrosFIDC.Criar(
            fundoId: fundoId,
            tipoFidc: tipoFidc,
            tiposRecebiveis: tiposRecebiveis,
            prazoMedioCarteira: prazoMedioCarteira,
            indiceSubordinacaoAlvo: indiceSubordinacaoAlvo,
            indiceSubordinacaoMinimo: indiceSubordinacaoMinimo,
            provisaoDevedoresDuvidosos: provisaoDevedoresDuvidosos,
            limiteConcentracaoCedente: limiteConcentracaoCedente,
            limiteConcentracaoSacado: limiteConcentracaoSacado,
            possuiCoobrigacao: true,
            percentualCoobrigacao: percentualCoobrigacao,
            registradoraRecebiveis: registradora,
            integracaoRegistradora: true,
            codigoRegistradora: codigoRegistradora);

        // Assert
        Assert.NotNull(parametros);
        Assert.Equal(prazoMedioCarteira, parametros.PrazoMedioCarteira);
        Assert.Equal(indiceSubordinacaoAlvo, parametros.IndiceSubordinacaoAlvo);
        Assert.Equal(indiceSubordinacaoMinimo, parametros.IndiceSubordinacaoMinimo);
        Assert.Equal(provisaoDevedoresDuvidosos, parametros.ProvisaoDevedoresDuvidosos);
        Assert.Equal(limiteConcentracaoCedente, parametros.LimiteConcentracaoCedente);
        Assert.Equal(limiteConcentracaoSacado, parametros.LimiteConcentracaoSacado);
        Assert.True(parametros.PossuiCoobrigacao);
        Assert.Equal(percentualCoobrigacao, parametros.PercentualCoobrigacao);
        Assert.Equal(registradora, parametros.RegistradoraRecebiveis);
        Assert.True(parametros.IntegracaoRegistradora);
        Assert.Equal(codigoRegistradora, parametros.CodigoRegistradora);
    }

    [Fact]
    public void Criar_SemTiposRecebiveis_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis>();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoParametrosFIDC.Criar(fundoId, tipoFidc, tiposRecebiveis));
        Assert.Contains("Tipos de recebíveis são obrigatórios", exception.Message);
    }

    [Fact]
    public void Criar_ComTiposRecebiveisNull_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoParametrosFIDC.Criar(fundoId, tipoFidc, null!));
        Assert.Contains("Tipos de recebíveis são obrigatórios", exception.Message);
    }

    [Fact]
    public void Criar_ComPrazoMedioCarteiraZero_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata };

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoParametrosFIDC.Criar(fundoId, tipoFidc, tiposRecebiveis, prazoMedioCarteira: 0));
        Assert.Contains("Prazo médio da carteira deve ser maior que zero", exception.Message);
    }

    [Fact]
    public void Criar_ComPercentualInvalido_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata };

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoParametrosFIDC.Criar(fundoId, tipoFidc, tiposRecebiveis, indiceSubordinacaoAlvo: 1.5m));
        Assert.Contains("Índice de subordinação alvo deve estar entre 0 e 1", exception.Message);
    }

    [Fact]
    public void Criar_ComCoobrigacaoSemPercentual_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata };

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoParametrosFIDC.Criar(fundoId, tipoFidc, tiposRecebiveis, possuiCoobrigacao: true));
        Assert.Contains("Percentual de coobrigação é obrigatório quando possui coobrigação", exception.Message);
    }

    [Fact]
    public void Criar_ComIntegracaoSemRegistradora_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata };

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoParametrosFIDC.Criar(fundoId, tipoFidc, tiposRecebiveis, integracaoRegistradora: true));
        Assert.Contains("Registradora de recebíveis é obrigatória quando há integração", exception.Message);
    }

    [Fact]
    public void Criar_ComCodigoRegistradoraMuitoLongo_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata };
        var codigoLongo = new string('A', 51); // 51 caracteres

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            FundoParametrosFIDC.Criar(fundoId, tipoFidc, tiposRecebiveis, codigoRegistradora: codigoLongo));
        Assert.Contains("Código da registradora deve ter no máximo 50 caracteres", exception.Message);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarParametros()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata };
        var parametros = FundoParametrosFIDC.Criar(fundoId, tipoFidc, tiposRecebiveis);

        var novoTipoFidc = TipoFIDC.NaoPadronizado;
        var novosTiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.CCB, TipoRecebiveis.Cheque };
        var novoPrazo = 360;

        // Act
        parametros.Atualizar(
            tipoFidc: novoTipoFidc,
            tiposRecebiveis: novosTiposRecebiveis,
            prazoMedioCarteira: novoPrazo,
            indiceSubordinacaoAlvo: 0.20m,
            indiceSubordinacaoMinimo: 0.15m,
            provisaoDevedoresDuvidosos: 0.03m,
            limiteConcentracaoCedente: 0.30m,
            limiteConcentracaoSacado: 0.35m,
            possuiCoobrigacao: false,
            percentualCoobrigacao: null,
            registradoraRecebiveis: null,
            integracaoRegistradora: false,
            codigoRegistradora: null);

        // Assert
        Assert.Equal(novoTipoFidc, parametros.TipoFidc);
        Assert.Equal(2, parametros.TiposRecebiveis.Count);
        Assert.Equal(novoPrazo, parametros.PrazoMedioCarteira);
        Assert.NotNull(parametros.UpdatedAt);
    }

    [Fact]
    public void Atualizar_SemTiposRecebiveis_DeveLancarExcecao()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var tipoFidc = TipoFIDC.Padronizado;
        var tiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata };
        var parametros = FundoParametrosFIDC.Criar(fundoId, tipoFidc, tiposRecebiveis);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            parametros.Atualizar(
                tipoFidc: tipoFidc,
                tiposRecebiveis: new List<TipoRecebiveis>(),
                prazoMedioCarteira: null,
                indiceSubordinacaoAlvo: null,
                indiceSubordinacaoMinimo: null,
                provisaoDevedoresDuvidosos: null,
                limiteConcentracaoCedente: null,
                limiteConcentracaoSacado: null,
                possuiCoobrigacao: false,
                percentualCoobrigacao: null,
                registradoraRecebiveis: null,
                integracaoRegistradora: false,
                codigoRegistradora: null));
        Assert.Contains("Tipos de recebíveis são obrigatórios", exception.Message);
    }

    #endregion
}
