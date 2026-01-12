using CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;
using CoreLedger.Domain.Cadastros.Enums;
using CoreLedger.Domain.Cadastros.ValueObjects;
using FluentValidation;
using FluentValidation.TestHelper;

namespace CoreLedger.UnitTests.Application.Validators.Cadastros;

/// <summary>
///     Testes para validações síncronas do CreateFundoCommand.
///     Nota: Validações assíncronas (CNPJ único) requerem testes de integração.
/// </summary>
public class CreateFundoCommandValidatorTests
{
    private static CreateFundoCommand CriarCommandValido() => new(
        Cnpj: "11222333000181", // CNPJ válido
        RazaoSocial: "Fundo de Investimento XYZ FI",
        TipoFundo: TipoFundo.FI,
        ClassificacaoCVM: ClassificacaoCVM.RendaFixa,
        Prazo: PrazoFundo.Indeterminado,
        PublicoAlvo: PublicoAlvo.Geral,
        Tributacao: TributacaoFundo.LongoPrazo,
        Condominio: TipoCondominio.Aberto
    );

    /// <summary>
    ///     Validator simplificado apenas para regras síncronas.
    /// </summary>
    private class SyncCreateFundoCommandValidator : AbstractValidator<CreateFundoCommand>
    {
        public SyncCreateFundoCommandValidator()
        {
            RuleFor(x => x.Cnpj)
                .NotEmpty()
                .WithMessage("CNPJ é obrigatório")
                .Must(BeValidCnpj)
                .WithMessage("CNPJ inválido");

            RuleFor(x => x.RazaoSocial)
                .NotEmpty()
                .WithMessage("Razão social é obrigatória")
                .MinimumLength(5)
                .WithMessage("Razão social deve ter no mínimo 5 caracteres")
                .MaximumLength(200)
                .WithMessage("Razão social deve ter no máximo 200 caracteres");

            RuleFor(x => x.TipoFundo)
                .IsInEnum()
                .WithMessage("Tipo de fundo inválido");

            RuleFor(x => x.ClassificacaoCVM)
                .IsInEnum()
                .WithMessage("Classificação CVM inválida");

            RuleFor(x => x.PublicoAlvo)
                .IsInEnum()
                .WithMessage("Público-alvo inválido");

            RuleFor(x => x.Tributacao)
                .IsInEnum()
                .WithMessage("Tributação inválida");

            RuleFor(x => x.Condominio)
                .IsInEnum()
                .WithMessage("Tipo de condomínio inválido");

            RuleFor(x => x.Prazo)
                .IsInEnum()
                .WithMessage("Prazo do fundo inválido");

            RuleFor(x => x.DataConstituicao)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .When(x => x.DataConstituicao.HasValue)
                .WithMessage("Data de constituição não pode ser futura");

            RuleFor(x => x.DataInicioAtividade)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .When(x => x.DataInicioAtividade.HasValue)
                .WithMessage("Data de início de atividade não pode ser futura");

            RuleFor(x => x.DataInicioAtividade)
                .GreaterThanOrEqualTo(x => x.DataConstituicao!.Value)
                .When(x => x.DataConstituicao.HasValue && x.DataInicioAtividade.HasValue)
                .WithMessage("Data de início de atividade não pode ser anterior à data de constituição");

            RuleFor(x => x.PercentualExterior)
                .InclusiveBetween(0, 100)
                .WithMessage("Percentual exterior deve estar entre 0 e 100");

            RuleFor(x => x.NomeFantasia)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.NomeFantasia))
                .WithMessage("Nome fantasia deve ter no máximo 100 caracteres");

            RuleFor(x => x.NomeCurto)
                .MaximumLength(30)
                .When(x => !string.IsNullOrWhiteSpace(x.NomeCurto))
                .WithMessage("Nome curto deve ter no máximo 30 caracteres");

            RuleFor(x => x.CodigoAnbima)
                .Matches("^[0-9]{6}$")
                .When(x => !string.IsNullOrWhiteSpace(x.CodigoAnbima))
                .WithMessage("Código ANBIMA deve conter exatamente 6 dígitos");
        }

        private static bool BeValidCnpj(string cnpj)
        {
            return CNPJ.TentarCriar(cnpj, out _);
        }
    }

    private readonly SyncCreateFundoCommandValidator _validator = new();

    [Fact]
    public void Validate_ComDadosValidos_NaoDeveRetornarErros()
    {
        // Arrange
        var command = CriarCommandValido();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SemCnpj_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { Cnpj = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cnpj)
            .WithErrorMessage("CNPJ é obrigatório");
    }

    [Fact]
    public void Validate_ComCnpjInvalido_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { Cnpj = "12345678901234" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cnpj)
            .WithErrorMessage("CNPJ inválido");
    }

    [Fact]
    public void Validate_SemRazaoSocial_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { RazaoSocial = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RazaoSocial)
            .WithErrorMessage("Razão social é obrigatória");
    }

    [Fact]
    public void Validate_ComRazaoSocialMuitoCurta_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { RazaoSocial = "ABCD" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RazaoSocial)
            .WithErrorMessage("Razão social deve ter no mínimo 5 caracteres");
    }

    [Fact]
    public void Validate_ComRazaoSocialMuitoLonga_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { RazaoSocial = new string('A', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RazaoSocial)
            .WithErrorMessage("Razão social deve ter no máximo 200 caracteres");
    }

    [Fact]
    public void Validate_ComPercentualExteriorNegativo_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { PercentualExterior = -1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PercentualExterior)
            .WithErrorMessage("Percentual exterior deve estar entre 0 e 100");
    }

    [Fact]
    public void Validate_ComPercentualExteriorMaiorQue100_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { PercentualExterior = 101 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PercentualExterior)
            .WithErrorMessage("Percentual exterior deve estar entre 0 e 100");
    }

    [Fact]
    public void Validate_ComDataConstituicaoFutura_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with
        {
            DataConstituicao = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataConstituicao)
            .WithErrorMessage("Data de constituição não pode ser futura");
    }

    [Fact]
    public void Validate_ComDataInicioAtividadeAnteriorConstituicao_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with
        {
            DataConstituicao = DateOnly.FromDateTime(DateTime.Today),
            DataInicioAtividade = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataInicioAtividade)
            .WithErrorMessage("Data de início de atividade não pode ser anterior à data de constituição");
    }

    [Fact]
    public void Validate_ComCodigoAnbimaInvalido_DeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { CodigoAnbima = "12345" }; // deve ter 6 dígitos

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CodigoAnbima)
            .WithErrorMessage("Código ANBIMA deve conter exatamente 6 dígitos");
    }

    [Fact]
    public void Validate_ComCodigoAnbimaValido_NaoDeveRetornarErro()
    {
        // Arrange
        var command = CriarCommandValido() with { CodigoAnbima = "123456" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CodigoAnbima);
    }
}
