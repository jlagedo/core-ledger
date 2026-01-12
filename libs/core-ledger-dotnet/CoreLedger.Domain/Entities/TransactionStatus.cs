using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     TransactionStatus domain entity representing transaction workflow states.
/// </summary>
public class TransactionStatus : BaseEntity
{
    private TransactionStatus()
    {
    }

    public string ShortDescription { get; private set; } = string.Empty;
    public string LongDescription { get; private set; } = string.Empty;

    /// <summary>
    ///     Factory method to create a new TransactionStatus with validation.
    /// </summary>
    public static TransactionStatus Create(string shortDescription, string longDescription)
    {
        ValidateDescriptions(shortDescription, longDescription);

        return new TransactionStatus
        {
            ShortDescription = shortDescription.Trim(),
            LongDescription = longDescription.Trim()
        };
    }

    /// <summary>
    ///     Updates the descriptions with validation.
    /// </summary>
    public void UpdateDescriptions(string shortDescription, string longDescription)
    {
        ValidateDescriptions(shortDescription, longDescription);

        ShortDescription = shortDescription.Trim();
        LongDescription = longDescription.Trim();
        SetUpdated();
    }

    private static void ValidateDescriptions(string shortDescription, string longDescription)
    {
        if (string.IsNullOrWhiteSpace(shortDescription))
            throw new DomainValidationException("Short description cannot be empty");

        if (shortDescription.Length > 50)
            throw new DomainValidationException("Short description cannot exceed 50 characters");

        if (string.IsNullOrWhiteSpace(longDescription))
            throw new DomainValidationException("Long description cannot be empty");

        if (longDescription.Length > 200)
            throw new DomainValidationException("Long description cannot exceed 200 characters");
    }
}