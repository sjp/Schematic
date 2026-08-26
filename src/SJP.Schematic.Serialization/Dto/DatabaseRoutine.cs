namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database routine, e.g. a stored procedure or a function.
/// </summary>
public sealed record DatabaseRoutine
{
    /// <summary>
    /// The name of the routine.
    /// </summary>
    public required Identifier RoutineName { get; init; }

    /// <summary>
    /// The definition of the routine.
    /// </summary>
    public required string Definition { get; init; }
}
