using System.Collections.Generic;

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

    /// <summary>
    /// The kind of routine, e.g. a procedure or a function.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before routines carried a kind still reads back,
    /// as an unknown kind.
    /// </remarks>
    public Core.RoutineType RoutineType { get; init; }

    /// <summary>
    /// The language the routine is written in. Absent when the source database records none.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// The parameters the routine declares. When <see cref="Overloads"/> is populated these are
    /// the first overload's parameters.
    /// </summary>
    public IEnumerable<DatabaseRoutineParameter> Parameters { get; init; } = [];

    /// <summary>
    /// The type of value the routine returns. Absent for a procedure, and for a routine whose
    /// return the source database did not describe.
    /// </summary>
    public DbType? ReturnType { get; init; }

    /// <summary>
    /// Every signature declared under the routine's name, when the name carries more than one.
    /// Empty otherwise, in which case <see cref="Parameters"/> and <see cref="ReturnType"/>
    /// describe the only signature.
    /// </summary>
    public IEnumerable<DatabaseRoutineOverload> Overloads { get; init; } = [];
}
