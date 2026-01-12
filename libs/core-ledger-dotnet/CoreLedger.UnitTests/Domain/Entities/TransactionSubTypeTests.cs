using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Entities;

/// <summary>
///     Unit tests for TransactionSubType domain entity business rules and invariants.
/// </summary>
public class TransactionSubTypeTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldCreateTransactionSubType()
    {
        // Arrange
        var typeId = 1;
        var shortDescription = "BUY";
        var longDescription = "Purchase of shares";

        // Act
        var subType = TransactionSubType.Create(typeId, shortDescription, longDescription);

        // Assert
        Assert.NotNull(subType);
        Assert.Equal(typeId, subType.TypeId);
        Assert.Equal(shortDescription, subType.ShortDescription);
        Assert.Equal(longDescription, subType.LongDescription);
        Assert.True((DateTime.UtcNow - subType.CreatedAt).TotalSeconds < 1);
        Assert.Null(subType.UpdatedAt);
    }

    [Fact]
    public void Create_WithMaxLengthDescriptions_ShouldSucceed()
    {
        // Arrange
        var typeId = 1;
        var shortDescription = new string('A', 50);
        var longDescription = new string('B', 200);

        // Act
        var subType = TransactionSubType.Create(typeId, shortDescription, longDescription);

        // Assert
        Assert.NotNull(subType);
        Assert.Equal(50, subType.ShortDescription.Length);
        Assert.Equal(200, subType.LongDescription.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidTypeId_ShouldThrowDomainValidationException(int typeId)
    {
        // Arrange
        var shortDescription = "BUY";
        var longDescription = "Purchase of shares";

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            TransactionSubType.Create(typeId, shortDescription, longDescription));
        Assert.Equal("TypeId must be a positive number", exception.Message);
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
            TransactionSubType.Create(1, shortDescription!, longDescription));
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
            TransactionSubType.Create(1, shortDescription, longDescription!));
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
            TransactionSubType.Create(1, shortDescription, longDescription));
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
            TransactionSubType.Create(1, shortDescription, longDescription));
        Assert.Equal("Long description cannot exceed 200 characters", exception.Message);
    }

    [Fact]
    public void Create_WithWhitespaceDescriptions_ShouldTrimAndSucceed()
    {
        // Arrange
        var typeId = 1;
        var shortDescription = "  SELL  ";
        var longDescription = "  Sale of shares  ";

        // Act
        var subType = TransactionSubType.Create(typeId, shortDescription, longDescription);

        // Assert
        Assert.Equal("SELL", subType.ShortDescription);
        Assert.Equal("Sale of shares", subType.LongDescription);
    }

    #endregion
}