using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Entities;

/// <summary>
///     Unit tests for TransactionType domain entity business rules and invariants.
/// </summary>
public class TransactionTypeTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidDescriptions_ShouldCreateTransactionType()
    {
        // Arrange
        var shortDescription = "EQUITY";
        var longDescription = "Equity securities trading";

        // Act
        var type = TransactionType.Create(shortDescription, longDescription);

        // Assert
        Assert.NotNull(type);
        Assert.Equal(shortDescription, type.ShortDescription);
        Assert.Equal(longDescription, type.LongDescription);
        Assert.True((DateTime.UtcNow - type.CreatedAt).TotalSeconds < 1);
        Assert.Null(type.UpdatedAt);
    }

    [Fact]
    public void Create_WithMaxLengthDescriptions_ShouldSucceed()
    {
        // Arrange
        var shortDescription = new string('A', 50);
        var longDescription = new string('B', 200);

        // Act
        var type = TransactionType.Create(shortDescription, longDescription);

        // Assert
        Assert.NotNull(type);
        Assert.Equal(50, type.ShortDescription.Length);
        Assert.Equal(200, type.LongDescription.Length);
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
            TransactionType.Create(shortDescription!, longDescription));
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
            TransactionType.Create(shortDescription, longDescription!));
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
            TransactionType.Create(shortDescription, longDescription));
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
            TransactionType.Create(shortDescription, longDescription));
        Assert.Equal("Long description cannot exceed 200 characters", exception.Message);
    }

    [Fact]
    public void Create_WithWhitespaceDescriptions_ShouldTrimAndSucceed()
    {
        // Arrange
        var shortDescription = "  DERIVATIVE_OPTION  ";
        var longDescription = "  Options derivative instruments  ";

        // Act
        var type = TransactionType.Create(shortDescription, longDescription);

        // Assert
        Assert.Equal("DERIVATIVE_OPTION", type.ShortDescription);
        Assert.Equal("Options derivative instruments", type.LongDescription);
    }

    #endregion
}