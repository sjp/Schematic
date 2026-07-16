using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines connection-bound operations for retrieving a relational database and its metadata.
/// </summary>
/// <remarks>
/// An implementation is constructed from (and bound to) a single <see cref="ISchematicConnection"/>.
/// For stateless, connection-independent vendor syntax (quoting, reserved keywords, column types),
/// see <see cref="IDatabaseDialect"/>.
/// </remarks>
public interface IRelationalDatabaseProvider
{
    /// <summary>
    /// Retrieves the set of identifier defaults for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A set of identifier defaults.</returns>
    Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the database version.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A version.</returns>
    Task<Version> GetDatabaseVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the database display version. Usually a more user-friendly form of the database version.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A descriptive version.</returns>
    Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a relational database for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database.</returns>
    Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a relational database comment provider for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A comment provider.</returns>
    Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default);
}
