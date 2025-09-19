using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A database table comment provider that always returns empty results.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableCommentProvider" />
public sealed class EmptyRelationalDatabaseTableCommentProvider : IRelationalDatabaseTableCommentProvider
{
    /// <summary>
    /// Enumerates all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of table comments.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IRelationalDatabaseTableComments>();

    /// <summary>
    /// Retrieves all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of table comments.</returns>
    public Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments2(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTableComments>>([]);

    /// <summary>
    /// Retrieves comments for a particular database table.
    /// </summary>
    /// <param name="tableName">The name of a database table.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IRelationalDatabaseTableComments}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return OptionAsync<IRelationalDatabaseTableComments>.None;
    }
}