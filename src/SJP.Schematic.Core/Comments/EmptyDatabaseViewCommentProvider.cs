using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A view comment provider that always returns empty results.
/// </summary>
/// <seealso cref="IDatabaseViewCommentProvider" />
public sealed class EmptyDatabaseViewCommentProvider : IDatabaseViewCommentProvider
{
    /// <summary>
    /// Enumerates all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of view comments.</returns>
    public IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseViewComments>();

    /// <summary>
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of view comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments2(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<IDatabaseViewComments>>([]);

    /// <summary>
    /// Retrieves comments for a particular database view.
    /// </summary>
    /// <param name="viewName">The name of a database view.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseViewComments}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return OptionAsync<IDatabaseViewComments>.None;
    }
}