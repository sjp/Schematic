using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// A database table provider that returns no tables. Not intended to be used directly.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public sealed class EmptyRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
{
    /// <summary>Gets all database tables. This will always be an empty collection.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IRelationalDatabaseTable>();

    /// <summary>
    /// Gets a database table. This will always be a 'none' result.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database table in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return OptionAsync<IRelationalDatabaseTable>.None;
    }
}