using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines comments available to database routines
/// </summary>
public interface IDatabaseRoutineComments
{
    /// <summary>
    /// The name of an <see cref="IDatabaseRoutine"/> instance.
    /// </summary>
    Identifier RoutineName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseRoutine"/> instance.
    /// </summary>
    /// <value>A comment, if available.</value>
    Option<string> Comment { get; }
}
