using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.Interfaces;
using CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CoreLedger.UnitTests.Application.UseCases.Cadastros.Fundos.Queries;

/// <summary>
///     Testes para VerificarCnpjDisponivelQueryHandler.
///     Nota: Testes com acesso a banco de dados (disponível/não disponível) requerem testes de integração.
///     Estes são testes unitários que validam apenas a lógica de formato de CNPJ.
/// </summary>
public class VerificarCnpjDisponivelQueryHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<VerificarCnpjDisponivelQueryHandler> _logger;
    private readonly VerificarCnpjDisponivelQueryHandler _handler;

    public VerificarCnpjDisponivelQueryHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _logger = Substitute.For<ILogger<VerificarCnpjDisponivelQueryHandler>>();
        _handler = new VerificarCnpjDisponivelQueryHandler(_context, _logger);
    }

    [Fact]
    public async Task Handle_ComCnpjInvalido_DeveRetornarCnpjInvalido()
    {
        // Arrange
        var cnpjInvalido = "12345678901234"; // CNPJ com checksum inválido
        var query = new VerificarCnpjDisponivelQuery(cnpjInvalido);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Disponivel);
        Assert.False(result.CnpjValido);
        Assert.Equal(cnpjInvalido, result.Cnpj);
        Assert.Equal("CNPJ inválido - formato ou dígitos verificadores incorretos", result.Mensagem);
    }

    [Fact]
    public async Task Handle_ComCnpjVazio_DeveRetornarCnpjInvalido()
    {
        // Arrange
        var query = new VerificarCnpjDisponivelQuery("");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Disponivel);
        Assert.False(result.CnpjValido);
        Assert.Equal("CNPJ inválido - formato ou dígitos verificadores incorretos", result.Mensagem);
    }

    [Theory]
    [InlineData("00000000000000")] // Todos zeros
    [InlineData("11111111111111")] // Todos iguais
    [InlineData("ABC12345678901")] // Letras no início (novo formato permite, mas precisa ser alfanumérico válido)
    [InlineData("1234567890123")]  // 13 dígitos (falta 1)
    [InlineData("123456789012345")] // 15 dígitos (excede)
    public async Task Handle_ComCnpjFormatoInvalido_DeveRetornarCnpjInvalido(string cnpjInvalido)
    {
        // Arrange
        var query = new VerificarCnpjDisponivelQuery(cnpjInvalido);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Disponivel);
        Assert.False(result.CnpjValido);
        Assert.Equal("CNPJ inválido - formato ou dígitos verificadores incorretos", result.Mensagem);
    }
}
