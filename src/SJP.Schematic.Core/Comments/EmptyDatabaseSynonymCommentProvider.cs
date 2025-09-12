using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A synonym comment provider that always returns empty results.
/// </summary>
/// <seealso cref="IDatabaseSynonymCommentProvider" />
public sealed class EmptyDatabaseSynonymCommentProvider : IDatabaseSynonymCommentProvider
{
    /// <summary>
    /// Retrieves all database synonym comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of synonym comments.</returns>
    public IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseSynonymComments>();

    /// <summary>
    /// Retrieves comments for a particular database synonym.
    /// </summary>
    /// <param name="synonymName">The name of a database synonym.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return OptionAsync<IDatabaseSynonymComments>.None;
    }
}