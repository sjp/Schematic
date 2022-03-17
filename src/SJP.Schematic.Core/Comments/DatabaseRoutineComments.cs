using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Comments for an <see cref="IDatabaseRoutine"/> instance.
/// </summary>
/// <seealso cref="IDatabaseRoutineComments" />
public class DatabaseRoutineComments : IDatabaseRoutineComments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRoutineComments"/> class.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="comment">The comment for the routine, if available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
    public DatabaseRoutineComments(Identifier routineName, Option<string> comment)
    {
        RoutineName = routineName ?? throw new ArgumentNullException(nameof(routineName));
        Comment = comment;
    }

    /// <summary>
    /// The name of an <see cref="IDatabaseRoutine" /> instance.
    /// </summary>
    public Identifier RoutineName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseRoutine" /> instance.
    /// </summary>
    /// <value>A comment, if available.</value>
    public Option<string> Comment { get; }
}
