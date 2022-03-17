using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle;

/// <summary>
/// Oracle limits the length of identifiers. This defines a validator for ensuring this is applied.
/// </summary>
public interface IOracleDatabaseIdentifierValidation
{
    /// <summary>
    /// Determines whether an identifier is valid for use in an Oracle database.
    /// </summary>
    /// <param name="identifier">An identifier.</param>
    /// <returns><c>true</c> if the identifier is valid for use in an Oracle database; otherwise, <c>false</c>.</returns>
    bool IsValidIdentifier(Identifier identifier);
}
