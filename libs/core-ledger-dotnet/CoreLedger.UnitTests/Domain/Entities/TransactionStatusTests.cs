using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Entities;

/// <summary>
///     Unit tests for TransactionStatus domain entity business rules and invariants.
/// </summary>
public class TransactionStatusTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidDescriptions_ShouldCreateTransactionStatus()
    {
        // Arrange
        var shortDescription = "NEW";
        var longDescription = "Trade created/imported but not yet confirmed";

        // Act
        var status = TransactionStatus.Create(shortDescription, longDescription);

        // Assert
        Assert.NotNull(status);
        Assert.Equal(shortDescription, status.ShortDescription);
        Assert.Equal(longDescription, status.LongDescription);
        Assert.True((DateTime.UtcNow - status.CreatedAt).TotalSeconds < 1);
        Assert.Null(status.UpdatedAt);
    }

    [Fact]
    public void Create_WithMaxLengthDescriptions_ShouldSucceed()
    {
        // Arrange
        var shortDescription = new string('A', 50);
        var longDescription = new string('B', 200);

        // Act
        var status = TransactionStatus.Create(shortDescription, longDescription);

        // Assert
        Assert.NotNull(status);
        Assert.Equal(50, status.ShortDescription.Length);
        Assert.Equal(200, status.LongDescription.Length);
    }

    [Theory]
    [InlineData(null, "Valid long description")]
    [InlineData("", "Valid long description")]
    [InlineData("   ", "Valid long description")]
    public void Create_WithEmptyShortDescription_ShouldThrowDomainValidationException(string? shortDescription,
        string longDescription)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            TransactionStatus.Create(shortDescription!, longDescription));
        Assert.Equal("Short description cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData("Valid short", null)]
    [InlineData("Valid short", "")]
    [InlineData("Valid short", "   ")]
    public void Create_WithEmptyLongDescription_ShouldThrowDomainValidationException(string shortDescription,
        string? longDescription)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            TransactionStatus.Create(shortDescription, longDescription!));
        Assert.Equal("Long description cannot be empty", exception.Message);
    }

    [Fact]
    public void Create_WithShortDescriptionExceeding50Characters_ShouldThrowDomainValidationException()
    {
        // Arrange
        var shortDescription = new string('A', 51);
        var longDescription = "Valid long description";

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            TransactionStatus.Create(shortDescription, longDescription));
        Assert.Equal("Short description cannot exceed 50 characters", exception.Message);
    }

    [Fact]
    public void Create_WithLongDescriptionExceeding200Characters_ShouldThrowDomainValidationException()
    {
        // Arrange
        var shortDescription = "VALID";
        var longDescription = new string('B', 201);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            TransactionStatus.Create(shortDescription, longDescription));
        Assert.Equal("Long description cannot exceed 200 characters", exception.Message);
    }

    [Fact]
    public void Create_WithWhitespaceDescriptions_ShouldTrimAndSucceed()
    {
        // Arrange
        var shortDescription = "  EXECUTED  ";
        var longDescription = "  Trade confirmed by broker  ";

        // Act
        var status = TransactionStatus.Create(shortDescription, longDescription);

        // Assert
        Assert.Equal("EXECUTED", status.ShortDescription);
        Assert.Equal("Trade confirmed by broker", status.LongDescription);
    }

    #endregion
}