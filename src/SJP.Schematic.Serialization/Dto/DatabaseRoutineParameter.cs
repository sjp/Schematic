namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized parameter declared by a database routine.
/// </summary>
public sealed record DatabaseRoutineParameter
{
    /// <summary>
    /// The name of the parameter. Absent when the parameter is positional.
    /// </summary>
    public Identifier? ParameterName { get; init; }

    /// <summary>
    /// The type of data the parameter accepts.
    /// </summary>
    public required DbType Type { get; init; }

    /// <summary>
    /// The direction that values flow through the parameter.
    /// </summary>
    public required Core.RoutineParameterDirection Direction { get; init; }

    /// <summary>
    /// The expression applied when no value is provided for the parameter, if any.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// The one-based position of the parameter within the routine's signature.
    /// </summary>
    public required int Ordinal { get; init; }
}
