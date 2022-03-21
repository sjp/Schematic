using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines an object which retrieves comments for database routines.
/// </summary>
/// <seealso cref="IDatabaseRoutine"/>
public interface IDatabaseRoutineCommentProvider
{
    /// <summary>
    /// Retrieves comments for a particular database routine.
    /// </summary>
    /// <param name="routineName">The name of a database routine.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}"/> instance which holds the value of the routine's comments, if available.</returns>
    OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of routine comments.</returns>
    IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments(CancellationToken cancellationToken = default);
}