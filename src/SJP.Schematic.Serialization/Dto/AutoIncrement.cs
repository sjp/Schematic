namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized auto-incrementing sequence attached to a column.
/// </summary>
public class AutoIncrement
{
    /// <summary>
    /// The value the sequence starts at.
    /// </summary>
    public required decimal InitialValue { get; init; }

    /// <summary>
    /// The amount the sequence advances by for each generated value.
    /// </summary>
    public required decimal Increment { get; init; }
}
