using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A sequence comment provider that always returns empty results.
/// </summary>
/// <seealso cref="IDatabaseSequenceCommentProvider" />
public sealed class EmptyDatabaseSequenceCommentProvider : IDatabaseSequenceCommentProvider
{
    /// <summary>
    /// Retrieves all database sequence comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of sequence comments.</returns>
    public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseSequenceComments>();

    /// <summary>
    /// Retrieves comments for a particular database sequence.
    /// </summary>
    /// <param name="sequenceName">The name of a database sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return OptionAsync<IDatabaseSequenceComments>.None;
    }
}