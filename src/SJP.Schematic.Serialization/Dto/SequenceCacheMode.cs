namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// The mode of a serialized sequence cache, recording which case of <see cref="Core.SequenceCache"/> it was written from.
/// </summary>
public enum SequenceCacheMode
{
    /// <summary>
    /// The database does not report how, or whether, the sequence's values are cached.
    /// </summary>
    Unknown,

    /// <summary>
    /// No values are pre-allocated, i.e. <c>NO CACHE</c> in SQL Server or <c>NOCACHE</c> in Oracle.
    /// Every value is generated on demand.
    /// </summary>
    None,

    /// <summary>
    /// A known number of values is pre-allocated, given by <see cref="DatabaseSequence.CacheSize"/>.
    /// </summary>
    Sized,

    /// <summary>
    /// Values are pre-allocated, but the database chooses how many and does not report the number,
    /// i.e. SQL Server's <c>CACHE</c> without a size.
    /// </summary>
    EngineDefault,
}
