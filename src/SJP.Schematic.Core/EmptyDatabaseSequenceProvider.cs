using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// A database sequence provider that returns no sequences. Not intended to be used directly.
/// </summary>
/// <seealso cref="IDatabaseSequenceProvider" />
public sealed class EmptyDatabaseSequenceProvider : IDatabaseSequenceProvider
{
    /// <summary>Gets all database sequences. This will always be an empty collection.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database sequences.</returns>
    public IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseSequence>();

    /// <summary>
    /// Gets a database sequence. This will always be a 'none' result.
    /// </summary>
    /// <param name="sequenceName">A database sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database sequence in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return OptionAsync<IDatabaseSequence>.None;
    }
}