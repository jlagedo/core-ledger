using CoreLedger.Application.DTOs.Wizard;
using CoreLedger.Application.Validators.Cadastros.Wizard;
using CoreLedger.Domain.Cadastros.Enums;
using FluentValidation.TestHelper;

namespace CoreLedger.UnitTests.Application.Validators.Cadastros.Wizard;

/// <summary>
///     Testes para WizardParametrosFidcValidator.
/// </summary>
public class WizardParametrosFidcValidatorTests
{
    private readonly WizardParametrosFidcValidator _validator = new();

    private static WizardParametrosFidcDto CriarDtoValido() => new(
        TipoFidc: TipoFIDC.Padronizado,
        TiposRecebiveis: new List<TipoRecebiveis> { TipoRecebiveis.Duplicata, TipoRecebiveis.CCB },
        PrazoMedioCarteira: 180,
        IndiceSubordinacaoAlvo: 0.15m,
        ProvisaoDevedoresDuvidosos: 0.05m,
        LimiteConcentracaoCedente: 0.20m,
        LimiteConcentracaoSacado: 0.15m,
        PossuiCoobrigacao: false,
        PercentualCoobrigacao: null,
        RatingMinimo: null,
        AgenciaRating: null,
        RegistradoraRecebiveis: Registradora.CERC,
        IntegracaoRegistradora: false,
        ContaRegistradora: null
    );

    [Fact]
    public void Validate_ComDadosValidos_NaoDeveRetornarErros()
    {
        // Arrange
        var dto = CriarDtoValido();

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SemTiposRecebiveis_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            TiposRecebiveis = new List<TipoRecebiveis>()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TiposRecebiveis)
            .WithErrorMessage("Tipos de recebíveis são obrigatórios.");
    }

    [Fact]
    public void Validate_ComPrazoMedioCarteiraZero_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            PrazoMedioCarteira = 0
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PrazoMedioCarteira)
            .WithErrorMessage("Prazo médio da carteira deve ser maior que zero.");
    }

    [Fact]
    public void Validate_ComPrazoMedioCarteiraNegativo_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            PrazoMedioCarteira = -10
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PrazoMedioCarteira)
            .WithErrorMessage("Prazo médio da carteira deve ser maior que zero.");
    }

    [Fact]
    public void Validate_ComIndiceSubordinacaoAlvoMaiorQue1_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            IndiceSubordinacaoAlvo = 1.5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IndiceSubordinacaoAlvo)
            .WithErrorMessage("Índice de subordinação alvo deve estar entre 0 e 1.");
    }

    [Fact]
    public void Validate_ComIndiceSubordinacaoAlvoNegativo_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            IndiceSubordinacaoAlvo = -0.1m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IndiceSubordinacaoAlvo)
            .WithErrorMessage("Índice de subordinação alvo deve estar entre 0 e 1.");
    }

    [Fact]
    public void Validate_ComProvisaoDevedoresDuvidososMaiorQue1_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            ProvisaoDevedoresDuvidosos = 1.2m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProvisaoDevedoresDuvidosos)
            .WithErrorMessage("Provisão para devedores duvidosos deve estar entre 0 e 1.");
    }

    [Fact]
    public void Validate_ComLimiteConcentracaoCedenteInvalido_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            LimiteConcentracaoCedente = 1.1m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LimiteConcentracaoCedente)
            .WithErrorMessage("Limite de concentração por cedente deve estar entre 0 e 1.");
    }

    [Fact]
    public void Validate_ComLimiteConcentracaoSacadoInvalido_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            LimiteConcentracaoSacado = -0.5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LimiteConcentracaoSacado)
            .WithErrorMessage("Limite de concentração por sacado deve estar entre 0 e 1.");
    }

    [Fact]
    public void Validate_ComPercentualCoobrigacaoMaiorQue1_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            PercentualCoobrigacao = 1.5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PercentualCoobrigacao)
            .WithErrorMessage("Percentual de coobrigação deve estar entre 0 e 1.");
    }

    [Fact]
    public void Validate_ComCoobrigacaoSemPercentual_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            PossuiCoobrigacao = true,
            PercentualCoobrigacao = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PercentualCoobrigacao)
            .WithErrorMessage("Percentual de coobrigação é obrigatório quando coobrigação é habilitada.");
    }

    [Fact]
    public void Validate_ComCoobrigacaoEPercentualValido_NaoDeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            PossuiCoobrigacao = true,
            PercentualCoobrigacao = 0.30m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ComRatingMinimoMuitoLongo_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            RatingMinimo = new string('A', 11) // > 10 caracteres
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RatingMinimo)
            .WithErrorMessage("Rating mínimo deve ter no máximo 10 caracteres.");
    }

    [Fact]
    public void Validate_ComAgenciaRatingMuitoLonga_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            AgenciaRating = new string('A', 51) // > 50 caracteres
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AgenciaRating)
            .WithErrorMessage("Agência de rating deve ter no máximo 50 caracteres.");
    }

    [Fact]
    public void Validate_ComContaRegistradoraMuitoLonga_DeveRetornarErro()
    {
        // Arrange
        var dto = CriarDtoValido() with
        {
            ContaRegistradora = new string('1', 51) // > 50 caracteres
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContaRegistradora)
            .WithErrorMessage("Conta na registradora deve ter no máximo 50 caracteres.");
    }
}
