using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A routine comment provider that always returns empty results.
/// </summary>
/// <seealso cref="IDatabaseRoutineCommentProvider" />
public sealed class EmptyDatabaseRoutineCommentProvider : IDatabaseRoutineCommentProvider
{
    /// <summary>
    /// Enumerates all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of routine comments.</returns>
    public IAsyncEnumerable<IDatabaseRoutineComments> EnumerateAllRoutineComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseRoutineComments>();

    /// <summary>
    /// Retrieves all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of routine comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default) => Empty.Tasks.RoutineComments;

    /// <summary>
    /// Retrieves comments for a particular database routine.
    /// </summary>
    /// <param name="routineName">The name of a database routine.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseRoutineComments}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return OptionAsync<IDatabaseRoutineComments>.None;
    }
}