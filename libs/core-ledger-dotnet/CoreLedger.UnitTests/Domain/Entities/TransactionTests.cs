using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Entities;

/// <summary>
///     Testes unitários para as regras de negócio da entidade de domínio Transação e invariantes.
/// </summary>
public class TransactionTests
{
    #region Testes de Criação - Validação de FundId

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidFundId_ShouldThrowDomainValidationException(int fundId)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId, 10, 5,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                "USD", 1, "test-user"));
        Assert.Equal("FundId deve ser um número positivo", exception.Message);
    }

    #endregion

    #region Testes de Criação - Validação de TransactionSubTypeId

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidTransactionSubTypeId_ShouldThrowDomainValidationException(int transactionSubTypeId)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                1, 10, transactionSubTypeId,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                "USD", 1, "test-user"));
        Assert.Equal("TransactionSubTypeId deve ser um número positivo", exception.Message);
    }

    #endregion

    #region Testes de Criação - Validação de StatusId

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidStatusId_ShouldThrowDomainValidationException(int statusId)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                1, 10, 5,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                "USD", statusId, "test-user"));
        Assert.Equal("StatusId deve ser um número positivo", exception.Message);
    }

    #endregion

    #region Testes de Criação - Validação de Preço

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowDomainValidationException()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                1, 10, 5,
                tradeDate, settleDate,
                100m, -0.01m, 5000m,
                "USD", 1, "test-user"));
        Assert.Equal("Preço não pode ser negativo", exception.Message);
    }

    #endregion

    #region Testes de Criação - Caminho Feliz

    [Fact]
    public void Create_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var fundId = 1;
        int? securityId = 10;
        var transactionSubTypeId = 5;
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);
        var quantity = 100.5m;
        var price = 50.25m;
        var amount = 5050.125m;
        var currency = "USD";
        var statusId = 1;

        // Act
        var transaction = Transaction.Create(
            fundId, securityId, transactionSubTypeId,
            tradeDate, settleDate, quantity, price, amount,
            currency, statusId, "test-user");

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(fundId, transaction.FundId);
        Assert.Equal(securityId, transaction.SecurityId);
        Assert.Equal(transactionSubTypeId, transaction.TransactionSubTypeId);
        Assert.Equal(tradeDate, transaction.TradeDate);
        Assert.Equal(settleDate, transaction.SettleDate);
        Assert.Equal(quantity, transaction.Quantity);
        Assert.Equal(price, transaction.Price);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(currency, transaction.Currency);
        Assert.Equal(statusId, transaction.StatusId);
        Assert.True((DateTime.UtcNow - transaction.CreatedAt).TotalSeconds < 1);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void Create_WithNullSecurityId_ShouldSucceed()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act
        var transaction = Transaction.Create(
            1, null, 5,
            tradeDate, settleDate,
            100m, 50m, 5000m,
            "USD", 1, "test-user");

        // Assert
        Assert.NotNull(transaction);
        Assert.Null(transaction.SecurityId);
    }

    [Fact]
    public void Create_WithSameTradeDateAndSettleDate_ShouldSucceed()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        // Act
        var transaction = Transaction.Create(
            1, 10, 5,
            date, date,
            100m, 50m, 5000m,
            "EUR", 1, "test-user");

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(date, transaction.TradeDate);
        Assert.Equal(date, transaction.SettleDate);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act
        var transaction = Transaction.Create(
            1, 10, 5,
            tradeDate, settleDate,
            100m, 0m, 0m,
            "GBP", 1, "test-user");

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(0m, transaction.Price);
    }

    #endregion

    #region Testes de Criação - Validação de Data

    [Fact]
    public void Create_WithTradeDateAfterSettleDate_ShouldThrowDomainValidationException()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                1, 10, 5,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                "USD", 1, "test-user"));
        Assert.Equal("Data de negociação deve estar na ou antes da data de liquidação", exception.Message);
    }

    [Fact]
    public void Create_WithSettleDateMoreThanOneYearInFuture_ShouldThrowDomainValidationException()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddYears(1).AddDays(1);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                1, 10, 5,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                "USD", 1, "test-user"));
        Assert.Equal("Data de liquidação não pode estar mais de 1 ano no futuro", exception.Message);
    }

    [Fact]
    public void Create_WithSettleDateExactlyOneYearInFuture_ShouldSucceed()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddYears(1);

        // Act
        var transaction = Transaction.Create(
            1, 10, 5,
            tradeDate, settleDate,
            100m, 50m, 5000m,
            "USD", 1, "test-user");

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(settleDate, transaction.SettleDate);
    }

    #endregion

    #region Testes de Criação - Validação de Moeda

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCurrency_ShouldThrowDomainValidationException(string? currency)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                1, 10, 5,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                currency!, 1, "test-user"));
        Assert.Equal("Moeda não pode estar vazia", exception.Message);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("U")]
    [InlineData("ABCDE")]
    public void Create_WithInvalidCurrencyLength_ShouldThrowDomainValidationException(string currency)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                1, 10, 5,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                currency, 1, "test-user"));
        Assert.Equal("Moeda deve ser um código ISO de 3 letras", exception.Message);
    }

    [Theory]
    [InlineData("US$")]
    [InlineData("123")]
    [InlineData("AB1")]
    public void Create_WithInvalidCurrencyFormat_ShouldThrowDomainValidationException(string currency)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                1, 10, 5,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                currency, 1, "test-user"));
        Assert.Equal("Moeda deve conter apenas letras (A-Z)", exception.Message);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("BRL")]
    public void Create_WithValidCurrencyCodes_ShouldSucceed(string currency)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act
        var transaction = Transaction.Create(
            1, 10, 5,
            tradeDate, settleDate,
            100m, 50m, 5000m,
            currency, 1, "test-user");

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(currency, transaction.Currency);
    }

    #endregion

    #region Testes de Atualização

    [Fact]
    public void Update_WithValidData_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);
        var transaction = Transaction.Create(
            1, 10, 5,
            tradeDate, settleDate,
            100m, 50m, 5000m,
            "USD", 1, "test-user");

        var originalCreatedAt = transaction.CreatedAt;
        var newFundId = 2;
        var newSecurityId = 20;
        var newSubTypeId = 6;
        var newTradeDate = tradeDate.AddDays(1);
        var newSettleDate = settleDate.AddDays(1);

        // Act
        transaction.Update(
            newFundId, newSecurityId, newSubTypeId,
            newTradeDate, newSettleDate,
            200m, 75m, 15000m,
            "EUR", 2);

        // Assert
        Assert.Equal(newFundId, transaction.FundId);
        Assert.Equal(newSecurityId, transaction.SecurityId);
        Assert.Equal(newSubTypeId, transaction.TransactionSubTypeId);
        Assert.Equal(newTradeDate, transaction.TradeDate);
        Assert.Equal(newSettleDate, transaction.SettleDate);
        Assert.Equal(200m, transaction.Quantity);
        Assert.Equal(75m, transaction.Price);
        Assert.Equal(15000m, transaction.Amount);
        Assert.Equal("EUR", transaction.Currency);
        Assert.Equal(2, transaction.StatusId);
        Assert.NotNull(transaction.UpdatedAt);
        Assert.True((DateTime.UtcNow - transaction.UpdatedAt.Value).TotalSeconds < 1);
        Assert.Equal(originalCreatedAt, transaction.CreatedAt);
    }

    [Fact]
    public void Update_WithInvalidData_ShouldThrowDomainValidationException()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);
        var transaction = Transaction.Create(
            1, 10, 5,
            tradeDate, settleDate,
            100m, 50m, 5000m,
            "USD", 1, "test-user");

        // Act & Assert - Invalid FundId
        var exception = Assert.Throws<DomainValidationException>(() =>
            transaction.Update(
                0, 10, 5,
                tradeDate, settleDate,
                100m, 50m, 5000m,
                "USD", 1));
        Assert.Equal("FundId deve ser um número positivo", exception.Message);
    }

    #endregion
}