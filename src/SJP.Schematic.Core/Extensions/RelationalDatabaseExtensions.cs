using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods used for working with <see cref="IRelationalDatabase"/> instances.
/// </summary>
public static class RelationalDatabaseExtensions
{
    /// <summary>
    /// Snapshots a relational database. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// Uses the <see cref="VerbatimIdentifierResolutionStrategy"/> to resolve identifiers by default (i.e. no casing/conversion behaviour).
    /// </summary>
    /// <param name="database">A relational database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database with the same data as <paramref name="database"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabase> SnapshotAsync(this IRelationalDatabase database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        return SnapshotAsyncCore(database, new VerbatimIdentifierResolutionStrategy(), cancellationToken);
    }

    /// <summary>
    /// Snapshots a relational database. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// </summary>
    /// <param name="database">A relational database.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database with the same data as <paramref name="database"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabase> SnapshotAsync(this IRelationalDatabase database, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        return SnapshotAsyncCore(database, identifierResolver, cancellationToken);
    }

    private static async Task<IRelationalDatabase> SnapshotAsyncCore(IRelationalDatabase database, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
    {
        var (
            tables,
            views,
            sequences,
            synonyms,
            routines
        ) = await TaskUtilities.WhenAll(
            database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            database.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            database.GetAllSynonyms(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            database.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask()
        ).ConfigureAwait(false);

        return new RelationalDatabase(
            database.IdentifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );
    }
}