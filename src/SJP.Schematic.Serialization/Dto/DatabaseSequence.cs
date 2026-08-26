namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database sequence.
/// </summary>
public class DatabaseSequence
{
    /// <summary>
    /// The name of the sequence.
    /// </summary>
    public required Identifier SequenceName { get; init; }

    /// <summary>
    /// The amount of values that are cached.
    /// </summary>
    public required int Cache { get; init; }

    /// <summary>
    /// Whether the sequence can cycle, generating duplicate values once its bounds are reached.
    /// </summary>
    public required bool Cycle { get; init; }

    /// <summary>
    /// The amount the sequence advances by for each generated value.
    /// </summary>
    public required decimal Increment { get; init; }

    /// <summary>
    /// The maximum value the sequence can generate, if any.
    /// </summary>
    public decimal? MaxValue { get; init; }

    /// <summary>
    /// The minimum value the sequence can generate, if any.
    /// </summary>
    public decimal? MinValue { get; init; }

    /// <summary>
    /// The value the sequence starts at.
    /// </summary>
    public required decimal Start { get; init; }
}
