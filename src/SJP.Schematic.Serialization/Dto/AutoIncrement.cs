namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized auto-incrementing sequence attached to a column.
/// </summary>
public sealed record AutoIncrement
{
    /// <summary>
    /// The value the sequence starts at.
    /// </summary>
    public required decimal InitialValue { get; init; }

    /// <summary>
    /// The amount the sequence advances by for each generated value.
    /// </summary>
    public required decimal Increment { get; init; }

    /// <summary>
    /// Whether a value supplied by an <c>INSERT</c> statement is accepted in place of a generated one.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before columns carried a generation strategy still
    /// reads back, as an unknown strategy.
    /// </remarks>
    public Core.IdentityGeneration Generation { get; init; }

    /// <summary>
    /// The smallest value the sequence generates, if the source database reported one.
    /// </summary>
    public decimal? MinValue { get; init; }

    /// <summary>
    /// The largest value the sequence generates, if the source database reported one.
    /// </summary>
    public decimal? MaxValue { get; init; }

    /// <summary>
    /// Whether the sequence restarts from its bound once exhausted.
    /// </summary>
    public bool Cycle { get; init; }

    /// <summary>
    /// The sequence object backing the column, if the source database names one.
    /// </summary>
    public Identifier? SequenceName { get; init; }
}
