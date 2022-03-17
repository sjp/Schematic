namespace SJP.Schematic.Core;

/// <summary>
/// Describes an autoincrementing sequence.
/// </summary>
public interface IAutoIncrement
{
    /// <summary>
    /// The starting value of the sequence.
    /// </summary>
    decimal InitialValue { get; }

    /// <summary>
    /// The value incremented to the current value for each new row.
    /// </summary>
    decimal Increment { get; }
}
