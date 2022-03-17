using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines core functionality specific to a given database dialect or vendor.
/// </summary>
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
    /// <returns><c>true</c> if the given text is a reserved keyword; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Retrieves the set of identifier defaults for the given database connection.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A set of identifier defaults.</returns>
    Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(ISchematicConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the database version.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A version.</returns>
    Task<Version> GetDatabaseVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the database display version. Usually a more user-friendly form of the database version.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A descriptive version.</returns>
    Task<string> GetDatabaseDisplayVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a relational database for the given dialect.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database.</returns>
    Task<IRelationalDatabase> GetRelationalDatabaseAsync(ISchematicConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a relational database comment provider for the given dialect.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A comment provider.</returns>
    Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(ISchematicConnection connection, CancellationToken cancellationToken = default);
}
