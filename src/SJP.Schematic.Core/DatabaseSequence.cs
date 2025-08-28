using System;
using System.ComponentModel;
using System.Diagnostics;
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
    /// <param name="start">The initial value.</param>
    /// <param name="increment">The increment for each new value of the sequence.</param>
    /// <param name="minValue">The minimum value of the sequence.</param>
    /// <param name="maxValue">The maximum value of the sequence.</param>
    /// <param name="cycle">Determines whether the sequence can cycle back to its starting values.</param>
    /// <param name="cacheSize">Size of the sequence cache.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Invalid combinations of values for <paramref name="minValue"/> and <paramref name="maxValue"/>.</exception>
    public DatabaseSequence(
        Identifier sequenceName,
        decimal start,
        decimal increment,
        Option<decimal> minValue,
        Option<decimal> maxValue,
        bool cycle,
        int cacheSize
    )
    {
        Name = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));
        Start = start;

        if (increment == 0)
            throw new ArgumentException("A non-zero increment is required", nameof(increment));
        Increment = increment;
        if (increment > 0)
        {
            minValue
                .Where(mv => mv > start)
                .IfSome(static _ => throw new ArgumentException("When a minimum value and positive increment is provided, the minimum value must not be larger than the starting value.", nameof(minValue)));

            maxValue
                .Where(mv => mv < start)
                .IfSome(static _ => throw new ArgumentException("When a maximum value and positive increment is provided, the maximum value must not be less than the starting value.", nameof(maxValue)));
        }
        else
        {
            minValue
                .Where(mv => mv < start)
                .IfSome(static _ => throw new ArgumentException("When a minimum value and negative increment is provided, the minimum value must not be less than the starting value.", nameof(minValue)));

            maxValue
                .Where(mv => mv > start)
                .IfSome(static _ => throw new ArgumentException("When a maximum value and negative increment is provided, the maximum value must not be larger than the starting value.", nameof(maxValue)));
        }

        if (cacheSize < 0)
            cacheSize = UnknownCacheSize;

        Cache = cacheSize;
        Cycle = cycle;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    /// <summary>
    /// The name of the database sequence.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// The amount of values that are cached.
    /// </summary>
    /// <value>The cache size.</value>
    public int Cache { get; }

    /// <summary>
    /// Determines whether the values in the sequence can cycle. When cycling is configured, a sequence can generate duplicate values.
    /// </summary>
    /// <value><c>true</c> if the sequence can cycle; otherwise, <c>false</c>.</value>
    public bool Cycle { get; }

    /// <summary>
    /// The increment size to use when generating a new value from the sequence.
    /// </summary>
    /// <value>The increment size.</value>
    public decimal Increment { get; }

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
    /// A constant value used when a cache size is unknown.
    /// </summary>
    /// <value>A constant to refer to an unknown cache size.</value>
    public static int UnknownCacheSize { get; } = -1;

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