using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle;

/// <summary>
/// An identifier length validator for Oracle database identifiers.
/// </summary>
/// <seealso cref="IOracleDatabaseIdentifierValidation" />
public class OracleDatabaseIdentifierLengthValidation : IOracleDatabaseIdentifierValidation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseIdentifierLengthValidation"/> class.
    /// </summary>
    /// <param name="maxLength">The maximum length that is permitted for identifiers.</param>
    /// <exception cref="ArgumentException">The maximum identifier length must be at least 1.</exception>
    public OracleDatabaseIdentifierLengthValidation(uint maxLength)
    {
        if (maxLength == 0)
            throw new ArgumentException("The maximum identifier length must be at least 1.", nameof(maxLength));

        MaxIdentifierLength = maxLength;
    }

    /// <summary>
    /// Gets the maximum length that is allowed for identifiers.
    /// </summary>
    /// <value>The maximum allowed identifier length.</value>
    protected virtual uint MaxIdentifierLength { get; }

    /// <summary>
    /// Determines whether an identifier is valid for use in an Oracle database.
    /// </summary>
    /// <param name="identifier">An identifier.</param>
    /// <returns><c>true</c> if the identifier is valid for use in an Oracle database; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">identifier</exception>
    public bool IsValidIdentifier(Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var components = new[] { identifier.Server, identifier.Database, identifier.Schema, identifier.LocalName };
        return components.All(IsIdentifierComponentValid);
    }

    // basically, are all of the characters ascii (equivalent to a byte for oracle's purposes)
    // and are all of the ascii characters no longer than the database's max identifier length
    private bool IsIdentifierComponentValid(string? component)
    {
        // let's assume null is equivalent to a null literal and always valid
        if (component == null)
            return true;

        var asciiByteCount = GetAsciiByteCount(component);
        return asciiByteCount == component.Length
            && asciiByteCount <= MaxIdentifierLength;
    }

    private static int GetAsciiByteCount(string component)
    {
        ArgumentNullException.ThrowIfNull(component);

        return component.Count(static c => c < 128);
    }
}