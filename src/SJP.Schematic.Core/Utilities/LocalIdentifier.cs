namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// Reduces identifiers to their local name without allocating when they already are.
/// </summary>
internal static class LocalIdentifier
{
    /// <summary>
    /// Returns an identifier holding only the local name of <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier">An identifier, possibly qualified by a server, database or schema.</param>
    /// <returns><paramref name="identifier"/> itself when it carries no qualifying components; otherwise a new identifier with only its local name.</returns>
    /// <remarks>
    /// <see cref="Identifier"/> constructors reject empty and whitespace components, so a <see langword="null" />
    /// component is the only way a component can be absent.
    /// </remarks>
    public static Identifier From(Identifier identifier) =>
        identifier.Server is null && identifier.Database is null && identifier.Schema is null
            ? identifier
            : new Identifier(identifier.LocalName);
}
