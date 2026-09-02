namespace SJP.Schematic.Core;

/// <summary>
/// Describes how a value flows through a routine parameter.
/// </summary>
/// <remarks>
/// A routine's return value is not a parameter. It is exposed by
/// <see cref="IDatabaseRoutine.ReturnType"/> instead, so there is no direction for it here.
/// </remarks>
public enum RoutineParameterDirection
{
    /// <summary>
    /// The parameter only supplies a value to the routine.
    /// </summary>
    Input,

    /// <summary>
    /// The parameter only returns a value from the routine.
    /// </summary>
    Output,

    /// <summary>
    /// The parameter supplies a value to the routine and returns one from it.
    /// </summary>
    InputOutput,
}
