namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized numeric precision.
/// </summary>
public class NumericPrecision
{
    /// <summary>
    /// The total number of digits that can be stored.
    /// </summary>
    public required int Precision { get; init; }

    /// <summary>
    /// The number of those digits that are held after the decimal point.
    /// </summary>
    public required int Scale { get; init; }
}
