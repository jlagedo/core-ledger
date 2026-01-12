using CoreLedger.Application.DTOs;
using CoreLedger.Application.Validators;
using CoreLedger.Domain.Cadastros.Enums;
using FluentValidation.TestHelper;

namespace CoreLedger.UnitTests.Application.Validators;

/// <summary>
///     Testes para CreateFundoParametrosFIDCDtoValidator.
/// </summary>
public class CreateFundoParametrosFIDCDtoValidatorTests
{
    private readonly CreateFundoParametrosFIDCDtoValidator _validator;

    public CreateFundoParametrosFIDCDtoValidatorTests()
    {
        _validator = new CreateFundoParametrosFIDCDtoValidator();
    }

    [Fact]
    public void Validate_ComDadosValidos_NaoDeveRetornarErros()
    {
        // Arrange
        var dto = new CreateFundoParametrosFIDCDto
        {
            FundoId = Guid.NewGuid(),
            TipoFidc = TipoFIDC.Padronizado,
            TiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata, TipoRecebiveis.CCB },
            PrazoMedioCarteira = 180,
            IndiceSubordinacaoAlvo = 0.15m,
            IndiceSubordinacaoMinimo = 0.10m,
            PossuiCoobrigacao = false,
            IntegracaoRegistradora = false
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SemFundoId_DeveRetornarErro()
    {
        // Arrange
        var dto = new CreateFundoParametrosFIDCDto
        {
            FundoId = Guid.Empty,
            TipoFidc = TipoFIDC.Padronizado,
            TiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FundoId);
    }

    [Fact]
    public void Validate_SemTiposRecebiveis_DeveRetornarErro()
    {
        // Arrange
        var dto = new CreateFundoParametrosFIDCDto
        {
            FundoId = Guid.NewGuid(),
            TipoFidc = TipoFIDC.Padronizado,
            TiposRecebiveis = new List<TipoRecebiveis>()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TiposRecebiveis);
    }

    [Fact]
    public void Validate_ComPrazoMedioCarteiraZero_DeveRetornarErro()
    {
        // Arrange
        var dto = new CreateFundoParametrosFIDCDto
        {
            FundoId = Guid.NewGuid(),
            TipoFidc = TipoFIDC.Padronizado,
            TiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata },
            PrazoMedioCarteira = 0
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PrazoMedioCarteira);
    }

    [Fact]
    public void Validate_ComPercentualInvalido_DeveRetornarErro()
    {
        // Arrange
        var dto = new CreateFundoParametrosFIDCDto
        {
            FundoId = Guid.NewGuid(),
            TipoFidc = TipoFIDC.Padronizado,
            TiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata },
            IndiceSubordinacaoAlvo = 1.5m // > 1
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IndiceSubordinacaoAlvo);
    }

    [Fact]
    public void Validate_ComCoobrigacaoSemPercentual_DeveRetornarErro()
    {
        // Arrange
        var dto = new CreateFundoParametrosFIDCDto
        {
            FundoId = Guid.NewGuid(),
            TipoFidc = TipoFIDC.Padronizado,
            TiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata },
            PossuiCoobrigacao = true,
            PercentualCoobrigacao = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PercentualCoobrigacao);
    }

    [Fact]
    public void Validate_ComIntegracaoSemRegistradora_DeveRetornarErro()
    {
        // Arrange
        var dto = new CreateFundoParametrosFIDCDto
        {
            FundoId = Guid.NewGuid(),
            TipoFidc = TipoFIDC.Padronizado,
            TiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata },
            IntegracaoRegistradora = true,
            RegistradoraRecebiveis = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegistradoraRecebiveis);
    }

    [Fact]
    public void Validate_ComCodigoRegistradoraMuitoLongo_DeveRetornarErro()
    {
        // Arrange
        var dto = new CreateFundoParametrosFIDCDto
        {
            FundoId = Guid.NewGuid(),
            TipoFidc = TipoFIDC.Padronizado,
            TiposRecebiveis = new List<TipoRecebiveis> { TipoRecebiveis.Duplicata },
            CodigoRegistradora = new string('A', 51) // > 50 caracteres
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CodigoRegistradora);
    }
}
