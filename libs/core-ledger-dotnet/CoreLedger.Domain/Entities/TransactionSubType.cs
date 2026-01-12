using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     TransactionSubType domain entity representing specific transaction operations within a type.
/// </summary>
public class TransactionSubType : BaseEntity
{
    private TransactionSubType()
    {
    }

    public int TypeId { get; private set; }
    public TransactionType? Type { get; private set; }
    public string ShortDescription { get; private set; } = string.Empty;
    public string LongDescription { get; private set; } = string.Empty;

    /// <summary>
    ///     Factory method to create a new TransactionSubType with validation.
    /// </summary>
    public static TransactionSubType Create(int typeId, string shortDescription, string longDescription)
    {
        ValidateTypeId(typeId);
        ValidateDescriptions(shortDescription, longDescription);

        return new TransactionSubType
        {
            TypeId = typeId,
            ShortDescription = shortDescription.Trim(),
            LongDescription = longDescription.Trim()
        };
    }

    /// <summary>
    ///     Updates the transaction subtype with validation.
    /// </summary>
    public void Update(int typeId, string shortDescription, string longDescription)
    {
        ValidateTypeId(typeId);
        ValidateDescriptions(shortDescription, longDescription);

        TypeId = typeId;
        ShortDescription = shortDescription.Trim();
        LongDescription = longDescription.Trim();
        SetUpdated();
    }

    private static void ValidateTypeId(int typeId)
    {
        if (typeId <= 0)
            throw new DomainValidationException("TypeId must be a positive number");
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