using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database sequence, used to generate numeric sequences.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
public interface IDatabaseSequence : IDatabaseEntity
{
    /// <summary>
    /// The amount of values that are cached.
    /// </summary>
    /// <value>The cache size.</value>
    int Cache { get; }

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