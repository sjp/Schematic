using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// A database view provider that returns no views. Not intended to be used directly.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public sealed class EmptyDatabaseViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Enumerates all database views. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> EnumerateAllViews(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseView>();

    /// <summary>
    /// Gets all database views. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database views.</returns>
    public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<IDatabaseView>>([]);

    /// <summary>
    /// Gets a database view. This will always be a 'none' result.
    /// </summary>
    /// <param name="viewName">A database view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database view in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return OptionAsync<IDatabaseView>.None;
    }
}