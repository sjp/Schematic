using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods used for working with <see cref="IRelationalDatabase"/> instances.
/// </summary>
public static class RelationalDatabaseSnapshotExtensions
{
    /// <summary>
    /// Snapshots a relational database. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// Snapshots all database objects and uses the <see cref="VerbatimIdentifierResolutionStrategy"/> to resolve identifiers by default (i.e. no casing/conversion behaviour).
    /// </summary>
    /// <param name="database">A relational database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database with the same data as <paramref name="database"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabase> SnapshotAsync(this IRelationalDatabase database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        return SnapshotAsyncCore(database, new RelationalDatabaseSnapshotOptions(), new VerbatimIdentifierResolutionStrategy(), cancellationToken);
    }

    /// <summary>
    /// Snapshots a relational database. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// Uses the <see cref="VerbatimIdentifierResolutionStrategy"/> to resolve identifiers by default (i.e. no casing/conversion behaviour).
    /// </summary>
    /// <param name="database">A relational database.</param>
    /// <param name="snapshotOptions">Options that are used to configure which database objects should be snapshotted.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database with the same data as <paramref name="database"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabase> SnapshotAsync(this IRelationalDatabase database, RelationalDatabaseSnapshotOptions snapshotOptions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(snapshotOptions);

        return SnapshotAsyncCore(database, snapshotOptions, new VerbatimIdentifierResolutionStrategy(), cancellationToken);
    }

    /// <summary>
    /// Snapshots a relational database. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// Snapshots all database all database objects by default.
    /// </summary>
    /// <param name="database">A relational database.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database with the same data as <paramref name="database"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> is <c>null</c> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabase> SnapshotAsync(this IRelationalDatabase database, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        return SnapshotAsyncCore(database, new RelationalDatabaseSnapshotOptions(), identifierResolver, cancellationToken);
    }

    /// <summary>
    /// Snapshots a relational database. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// </summary>
    /// <param name="database">A relational database.</param>
    /// <param name="snapshotOptions">Options that are used to configure which database objects should be snapshotted.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database with the same data as <paramref name="database"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> or <paramref name="snapshotOptions"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabase> SnapshotAsync(this IRelationalDatabase database, RelationalDatabaseSnapshotOptions snapshotOptions, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        return SnapshotAsyncCore(database, snapshotOptions, identifierResolver, cancellationToken);
    }

    private static async Task<IRelationalDatabase> SnapshotAsyncCore(IRelationalDatabase database, RelationalDatabaseSnapshotOptions snapshotOptions, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
    {
        var tablesTask = snapshotOptions.IncludeTables
            ? database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IRelationalDatabaseTable>>([]);
        var viewsTask = snapshotOptions.IncludeViews
            ? database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IDatabaseView>>([]);
        var sequencesTask = snapshotOptions.IncludeSequences
            ? database.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IDatabaseSequence>>([]);
        var synonymsTask = snapshotOptions.IncludeSynonyms
            ? database.GetAllSynonyms(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IDatabaseSynonym>>([]);
        var routinesTask = snapshotOptions.IncludeRoutines
            ? database.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IDatabaseRoutine>>([]);

        var (
            tables,
            views,
            sequences,
            synonyms,
            routines
        ) = await TaskUtilities.WhenAll(
            tablesTask,
            viewsTask,
            sequencesTask,
            synonymsTask,
            routinesTask
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