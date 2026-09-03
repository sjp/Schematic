namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database sequence.
/// </summary>
public sealed record DatabaseSequence
{
    /// <summary>
    /// The name of the sequence.
    /// </summary>
    public required Identifier SequenceName { get; init; }

    /// <summary>
    /// The type of the values the sequence generates.
    /// </summary>
    public required DbType Type { get; init; }

    /// <summary>
    /// Describes whether the database pre-allocates values for the sequence.
    /// </summary>
    public required Core.SequenceCacheMode CacheMode { get; init; }

    /// <summary>
    /// The number of values that are pre-allocated, if the database reports a size.
    /// </summary>
    public int? CacheSize { get; init; }

    /// <summary>
    /// Whether values are generated in the order they are requested.
    /// </summary>
    public required bool IsOrdered { get; init; }

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
