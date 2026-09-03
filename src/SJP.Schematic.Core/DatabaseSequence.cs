using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database sequence.
/// </summary>
/// <seealso cref="IDatabaseSequence" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseSequence : IDatabaseSequence
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSequence"/> class.
    /// </summary>
    /// <param name="sequenceName">Name of the sequence.</param>
    /// <param name="type">The type of the values the sequence generates.</param>
    /// <param name="start">The initial value.</param>
    /// <param name="increment">The increment for each new value of the sequence.</param>
    /// <param name="minValue">The minimum value of the sequence.</param>
    /// <param name="maxValue">The maximum value of the sequence.</param>
    /// <param name="cycle">Determines whether the sequence can cycle back to its starting values.</param>
    /// <param name="cacheMode">Describes whether the database pre-allocates values for the sequence.</param>
    /// <param name="cacheSize">The number of pre-allocated values. Ignored unless <paramref name="cacheMode"/> is <see cref="SequenceCacheMode.Sized"/>.</param>
    /// <param name="isOrdered">Determines whether values are generated in the order they are requested.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> or <paramref name="type"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="increment"/> is zero, or <paramref name="minValue"/> or <paramref name="maxValue"/> does not contain <paramref name="start"/>, or <paramref name="cacheMode"/> is not a valid enum.</exception>
    public DatabaseSequence(
        Identifier sequenceName,
        IDbType type,
        decimal start,
        decimal increment,
        Option<decimal> minValue,
        Option<decimal> maxValue,
        bool cycle,
        SequenceCacheMode cacheMode,
        Option<int> cacheSize,
        bool isOrdered
    )
    {
        Name = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Start = start;

        if (increment == 0)
            throw new ArgumentException("A non-zero increment is required", nameof(increment));
        Increment = increment;

        if (!cacheMode.IsValid())
            throw new ArgumentException($"The {nameof(SequenceCacheMode)} provided must be a valid enum.", nameof(cacheMode));

        // The starting value must lie within the bounds of the sequence, regardless of
        // the direction of travel. A descending sequence typically starts at its maximum
        // value and moves down towards its minimum value.
        minValue
            .Where(mv => mv > start)
            .IfSome(static _ => throw new ArgumentException("When a minimum value is provided, the minimum value must not be larger than the starting value.", nameof(minValue)));

        maxValue
            .Where(mv => mv < start)
            .IfSome(static _ => throw new ArgumentException("When a maximum value is provided, the maximum value must not be less than the starting value.", nameof(maxValue)));

        CacheMode = cacheMode;
        // a cache size only describes a cache whose size the database reports, so it is dropped
        // rather than kept alongside a mode that contradicts it
        CacheSize = cacheMode == SequenceCacheMode.Sized ? cacheSize : Option<int>.None;
        Cycle = cycle;
        IsOrdered = isOrdered;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    /// <summary>
    /// The name of the database sequence.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// The type of the values that the sequence generates.
    /// </summary>
    /// <value>An integral or numeric data type.</value>
    public IDbType Type { get; }

    /// <summary>
    /// Describes whether the database pre-allocates values for the sequence.
    /// </summary>
    /// <value>The cache mode.</value>
    public SequenceCacheMode CacheMode { get; }

    /// <summary>
    /// The number of values that are pre-allocated, available only when <see cref="CacheMode"/> is <see cref="SequenceCacheMode.Sized"/>.
    /// </summary>
    /// <value>If available, the cache size.</value>
    public Option<int> CacheSize { get; }

    /// <summary>
    /// Determines whether the values in the sequence can cycle. When cycling is configured, a sequence can generate duplicate values.
    /// </summary>
    /// <value><see langword="true" /> if the sequence can cycle; otherwise, <see langword="false" />.</value>
    public bool Cycle { get; }

    /// <summary>
    /// The increment size to use when generating a new value from the sequence.
    /// </summary>
    /// <value>The increment size.</value>
    public decimal Increment { get; }

    /// <summary>
    /// Determines whether values are guaranteed to be generated in the order they are requested.
    /// </summary>
    /// <value><see langword="true" /> if the sequence is ordered; otherwise, <see langword="false" />.</value>
    public bool IsOrdered { get; }

    /// <summary>
    /// If available, represents the maximum value that the sequence can generate.
    /// </summary>
    /// <value>If available, the maximum value.</value>
    public Option<decimal> MaxValue { get; }

    /// <summary>
    /// If available, represents the minimum value that the sequence can generate.
    /// </summary>
    /// <value>If available, the minimum value.</value>
    public Option<decimal> MinValue { get; }

    /// <summary>
    /// The starting value of the sequence.
    /// </summary>
    /// <value>The initial value.</value>
    public decimal Start { get; }

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("Sequence: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}
