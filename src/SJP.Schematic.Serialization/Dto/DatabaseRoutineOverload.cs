using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized signature of a database routine that shares its name with other signatures.
/// </summary>
public sealed record DatabaseRoutineOverload
{
    /// <summary>
    /// The definition of this signature alone.
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// The parameters this signature declares.
    /// </summary>
    public required IEnumerable<DatabaseRoutineParameter> Parameters { get; init; }

    /// <summary>
    /// The type of value this signature returns. Absent for a procedure.
    /// </summary>
    public DbType? ReturnType { get; init; }
}
