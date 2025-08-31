using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods used for working with <see cref="IRelationalDatabase"/> instances.
/// </summary>
public static class RelationalDatabaseCommentProviderSnapshotExtensions
{
    /// <summary>
    /// Snapshots a relational database comment provider. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// Snapshots all database object comments and uses the <see cref="VerbatimIdentifierResolutionStrategy"/> to resolve identifiers by default (i.e. no casing/conversion behaviour).
    /// </summary>
    /// <param name="databaseComments">A relational database comment provider.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database comment provider with the same data as <paramref name="databaseComments"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="databaseComments"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabaseCommentProvider> SnapshotAsync(this IRelationalDatabaseCommentProvider databaseComments, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseComments);

        return SnapshotAsyncCore(databaseComments, new RelationalDatabaseCommentProviderSnapshotOptions(), new VerbatimIdentifierResolutionStrategy(), cancellationToken);
    }

    /// <summary>
    /// Snapshots a relational database comment provider. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// Uses the <see cref="VerbatimIdentifierResolutionStrategy"/> to resolve identifiers by default (i.e. no casing/conversion behaviour).
    /// </summary>
    /// <param name="databaseComments">A relational database comment provider.</param>
    /// <param name="snapshotOptions">Options that are used to configure which database object comments should be snapshotted.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database comment provider with the same data as <paramref name="databaseComments"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="databaseComments"/> or <paramref name="snapshotOptions"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabaseCommentProvider> SnapshotAsync(this IRelationalDatabaseCommentProvider databaseComments, RelationalDatabaseCommentProviderSnapshotOptions snapshotOptions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseComments);
        ArgumentNullException.ThrowIfNull(snapshotOptions);

        return SnapshotAsyncCore(databaseComments, snapshotOptions, new VerbatimIdentifierResolutionStrategy(), cancellationToken);
    }

    /// <summary>
    /// Snapshots a relational database comment provider. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// Snapshots all database object comments by default.
    /// </summary>
    /// <param name="databaseComments">A relational database comment provider.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database comment provider with the same data as <paramref name="databaseComments"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="databaseComments"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabaseCommentProvider> SnapshotAsync(this IRelationalDatabaseCommentProvider databaseComments, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseComments);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        return SnapshotAsyncCore(databaseComments, new RelationalDatabaseCommentProviderSnapshotOptions(), identifierResolver, cancellationToken);
    }

    /// <summary>
    /// Snapshots a relational database comment provider. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// </summary>
    /// <param name="databaseComments">A relational database comment provider.</param>
    /// <param name="snapshotOptions">Options that are used to configure which database object comments should be snapshotted.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database comment provider with the same data as <paramref name="databaseComments"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="databaseComments"/> or <paramref name="snapshotOptions"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabaseCommentProvider> SnapshotAsync(this IRelationalDatabaseCommentProvider databaseComments, RelationalDatabaseCommentProviderSnapshotOptions snapshotOptions, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseComments);
        ArgumentNullException.ThrowIfNull(snapshotOptions);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        return SnapshotAsyncCore(databaseComments, snapshotOptions, identifierResolver, cancellationToken);
    }

    private static async Task<IRelationalDatabaseCommentProvider> SnapshotAsyncCore(IRelationalDatabaseCommentProvider databaseComments, RelationalDatabaseCommentProviderSnapshotOptions snapshotOptions, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
    {
        var tableCommentsTask = snapshotOptions.IncludeTableComments
            ? databaseComments.GetAllTableComments(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IRelationalDatabaseTableComments>>([]);
        var viewCommentsTask = snapshotOptions.IncludeViewComments
            ? databaseComments.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IDatabaseViewComments>>([]);
        var sequenceCommentsTask = snapshotOptions.IncludeSequenceComments
            ? databaseComments.GetAllSequenceComments(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IDatabaseSequenceComments>>([]);
        var synonymCommentsTask = snapshotOptions.IncludeSynonymComments
            ? databaseComments.GetAllSynonymComments(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IDatabaseSynonymComments>>([]);
        var routineCommentsTask = snapshotOptions.IncludeRoutineComments
            ? databaseComments.GetAllRoutineComments(cancellationToken).ToListAsync(cancellationToken).AsTask()
            : Task.FromResult<List<IDatabaseRoutineComments>>([]);

        var (
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        ) = await TaskUtilities.WhenAll(
            tableCommentsTask,
            viewCommentsTask,
            sequenceCommentsTask,
            synonymCommentsTask,
            routineCommentsTask
        ).ConfigureAwait(false);

        return new RelationalDatabaseCommentProvider(
            databaseComments.IdentifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );
    }
}