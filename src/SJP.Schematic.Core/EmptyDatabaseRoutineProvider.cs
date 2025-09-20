using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// A database routine provider that returns no routines. Not intended to be used directly.
/// </summary>
/// <seealso cref="IDatabaseRoutineProvider" />
public sealed class EmptyDatabaseRoutineProvider : IDatabaseRoutineProvider
{
    /// <summary>
    /// Enumerates all database routines. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseRoutine>();

    /// <summary>
    /// Gets all database routines. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database routines.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines2(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<IDatabaseRoutine>>([]);

    /// <summary>
    /// Gets a database routine. This will always be a 'none' result.
    /// </summary>
    /// <param name="routineName">A database routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return OptionAsync<IDatabaseRoutine>.None;
    }
}