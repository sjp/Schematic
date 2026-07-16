namespace SJP.Schematic.Core;

/// <summary>
/// Defines the SQL grammar and syntax rules specific to a given database dialect or vendor.
/// </summary>
/// <remarks>
/// This describes vendor syntax only (quoting, reserved keywords, column types, dependency parsing).
/// It is stateless and does not require a database connection. For connection-bound operations
/// (retrieving a relational database, its version, or its identifier defaults), see <see cref="IRelationalDatabaseProvider"/>.
/// </remarks>
public interface IDatabaseDialect
{
    /// <summary>
    /// Quotes a string identifier, e.g. a column name.
    /// </summary>
    /// <param name="identifier">An identifier.</param>
    /// <returns>A quoted identifier.</returns>
    string QuoteIdentifier(string identifier);

    /// <summary>
    /// Quotes a qualified name.
    /// </summary>
    /// <param name="name">An object name.</param>
    /// <returns>A quoted name.</returns>
    string QuoteName(Identifier name);

    /// <summary>
    /// Determines whether the given text is a reserved keyword.
    /// </summary>
    /// <param name="text">A piece of text.</param>
    /// <returns><see langword="true" /> if the given text is a reserved keyword; otherwise, <see langword="false" />.</returns>
    bool IsReservedKeyword(string text);

    /// <summary>
    /// Gets a database column data type provider.
    /// </summary>
    /// <value>The type provider.</value>
    IDbTypeProvider TypeProvider { get; }

    /// <summary>
    /// Gets a dependency provider, that parses text for object dependencies.
    /// </summary>
    /// <returns>A dependency provider.</returns>
    IDependencyProvider GetDependencyProvider();
}