using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database sequence provider that retrieves sequence information for a database.
/// </summary>
public interface IDatabaseSequenceProvider
{
    /// <summary>
    /// Gets a database sequence.
    /// </summary>
    /// <param name="sequenceName">A database sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
    OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all database sequences.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database sequences.</returns>
    IAsyncEnumerable<IDatabaseSequence> EnumerateAllSequences(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all database sequences.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database sequences.</returns>
    Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences2(CancellationToken cancellationToken = default);
}