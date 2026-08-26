namespace SJP.Schematic.Serialization.Dto.Comments;

/// <summary>
/// The serialized comments attached to a database routine.
/// </summary>
public sealed record DatabaseRoutineComments
{
    /// <summary>
    /// The name of the routine the comments are attached to.
    /// </summary>
    public required Identifier RoutineName { get; init; }

    /// <summary>
    /// The comment attached to the routine, if any.
    /// </summary>
    public string? Comment { get; init; }
}
