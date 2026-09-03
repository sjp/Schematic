using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database sequence, used to generate numeric sequences.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
public interface IDatabaseSequence : IDatabaseEntity
{
    /// <summary>
    /// The type of the values that the sequence generates.
    /// </summary>
    /// <value>An integral or numeric data type.</value>
    IDbType Type { get; }

    /// <summary>
    /// Describes whether the database pre-allocates values for the sequence.
    /// </summary>
    /// <value>The cache mode.</value>
    SequenceCacheMode CacheMode { get; }

    /// <summary>
    /// The number of values that are pre-allocated, available only when <see cref="CacheMode"/> is <see cref="SequenceCacheMode.Sized"/>.
    /// </summary>
    /// <value>If available, the cache size.</value>
    Option<int> CacheSize { get; }

    /// <summary>
    /// Determines whether the values in the sequence can cycle. When cycling is configured, a sequence can generate duplicate values.
    /// </summary>
    /// <value><see langword="true" /> if the sequence can cycle; otherwise, <see langword="false" />.</value>
    bool Cycle { get; }

    /// <summary>
    /// The increment size to use when generating a new value from the sequence.
    /// </summary>
    /// <value>The increment size.</value>
    decimal Increment { get; }

    /// <summary>
    /// Determines whether values are guaranteed to be generated in the order they are requested.
    /// </summary>
    /// <value><see langword="true" /> if the sequence is ordered; otherwise, <see langword="false" />.</value>
    /// <remarks>
    /// Only Oracle, whose sequences may be shared by the instances of a cluster, distinguishes an
    /// ordered sequence from an unordered one. Elsewhere a sequence is served by a single node and
    /// is therefore always ordered.
    /// </remarks>
    bool IsOrdered { get; }

    /// <summary>
    /// If available, represents the maximum value that the sequence can generate.
    /// </summary>
    /// <value>If available, the maximum value.</value>
    Option<decimal> MaxValue { get; }

    /// <summary>
    /// If available, represents the minimum value that the sequence can generate.
    /// </summary>
    /// <value>If available, the minimum value.</value>
    Option<decimal> MinValue { get; }

    /// <summary>
    /// The starting value of the sequence.
    /// </summary>
    /// <value>The initial value.</value>
    decimal Start { get; }
}
