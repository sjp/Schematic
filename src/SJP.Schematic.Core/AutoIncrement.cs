using System;
using System.ComponentModel;
using EnumsNET;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// A description of an auto-incrementing sequence.
/// </summary>
public sealed class AutoIncrement : IAutoIncrement, IEquatable<AutoIncrement>, IEquatable<IAutoIncrement>
{
    /// <summary>
    /// Creates a description of an auto-incrementing sequence that generates a value only when
    /// one is not supplied, and whose bounds and backing sequence are not known.
    /// </summary>
    /// <param name="initialValue">The starting value of the sequence.</param>
    /// <param name="increment">The value incremented to the current value on each new row.</param>
    /// <exception cref="ArgumentException"><paramref name="increment"/> is zero.</exception>
    public AutoIncrement(decimal initialValue, decimal increment)
        : this(initialValue, increment, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None)
    {
    }

    /// <summary>
    /// Creates a description of an auto-incrementing sequence.
    /// </summary>
    /// <param name="initialValue">The starting value of the sequence.</param>
    /// <param name="increment">The value incremented to the current value on each new row.</param>
    /// <param name="generation">Describes whether a supplied value is accepted in place of a generated one.</param>
    /// <param name="minValue">The smallest value the sequence generates, if known.</param>
    /// <param name="maxValue">The largest value the sequence generates, if known.</param>
    /// <param name="cycle">Whether the sequence restarts from its bound once exhausted.</param>
    /// <param name="sequenceName">The sequence object backing the column, if the database names one.</param>
    /// <exception cref="ArgumentException"><paramref name="increment"/> is zero, or <paramref name="generation"/> is not a valid enum.</exception>
    public AutoIncrement(
        decimal initialValue,
        decimal increment,
        IdentityGeneration generation,
        Option<decimal> minValue,
        Option<decimal> maxValue,
        bool cycle,
        Option<Identifier> sequenceName
    )
    {
        if (increment == 0)
            throw new ArgumentException("The increment value must be non-zero.", nameof(increment));
        if (!generation.IsValid())
            throw new ArgumentException($"The {nameof(IdentityGeneration)} provided must be a valid enum.", nameof(generation));

        InitialValue = initialValue;
        Increment = increment;
        Generation = generation;
        MinValue = minValue;
        MaxValue = maxValue;
        Cycle = cycle;
        SequenceName = sequenceName;
    }

    /// <summary>
    /// The starting value of the sequence.
    /// </summary>
    public decimal InitialValue { get; }

    /// <summary>
    /// The value incremented to the current value for each new row.
    /// </summary>
    public decimal Increment { get; }

    /// <summary>
    /// Describes whether a value supplied by an <c>INSERT</c> statement is accepted in place of a
    /// generated one.
    /// </summary>
    /// <value>A generation strategy, or <see cref="IdentityGeneration.Unknown"/> when the database does not report one.</value>
    public IdentityGeneration Generation { get; }

    /// <summary>
    /// The smallest value the sequence generates.
    /// </summary>
    /// <value>A minimum value, if the database reports one.</value>
    public Option<decimal> MinValue { get; }

    /// <summary>
    /// The largest value the sequence generates.
    /// </summary>
    /// <value>A maximum value, if the database reports one.</value>
    public Option<decimal> MaxValue { get; }

    /// <summary>
    /// Whether the sequence restarts from its bound once exhausted, instead of failing.
    /// </summary>
    /// <value><see langword="true"/> if the sequence cycles; otherwise, <see langword="false"/>.</value>
    public bool Cycle { get; }

    /// <summary>
    /// The sequence object backing the column, where the database implements the column with one.
    /// </summary>
    /// <value>A sequence name, if the column is backed by a sequence that the database names.</value>
    public Option<Identifier> SequenceName { get; }

    /// <summary>
    /// Indicates whether the current <see cref="AutoIncrement"/> object is equal to another <see cref="AutoIncrement"/> instance.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current <see cref="AutoIncrement"/> object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(AutoIncrement? other) => Equals((IAutoIncrement?)other);

    /// <summary>
    /// Indicates whether the current <see cref="AutoIncrement"/> object is equal to an <see cref="IAutoIncrement"/> instance.
    /// </summary>
    /// <param name="other">An <see cref="IAutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the current <see cref="AutoIncrement"/> object is equal to <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(IAutoIncrement? other)
    {
        if (other == null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return InitialValue == other.InitialValue
            && Increment == other.Increment
            && Generation == other.Generation
            && MinValue == other.MinValue
            && MaxValue == other.MaxValue
            && Cycle == other.Cycle
            && SequenceName == other.SequenceName;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object" />, is equal to this <see cref="AutoIncrement"/> instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns><see langword="true" /> if the specified <see cref="object" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => Equals(obj as IAutoIncrement);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => HashCode.Combine(InitialValue, Increment, Generation, MinValue, MaxValue, Cycle, SequenceName);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left <see cref="AutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="AutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(AutoIncrement? left, AutoIncrement? right)
    {
        return left is null ? right is null : left.Equals(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left <see cref="AutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="AutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is not equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(AutoIncrement? left, AutoIncrement? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left <see cref="IAutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="AutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(IAutoIncrement? left, AutoIncrement? right)
    {
        return right is null ? left is null : right.Equals(left);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left <see cref="IAutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="AutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is not equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(IAutoIncrement? left, AutoIncrement? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left <see cref="AutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="IAutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(AutoIncrement? left, IAutoIncrement? right)
    {
        return left is null ? right is null : left.Equals(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left <see cref="AutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="IAutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is not equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(AutoIncrement? left, IAutoIncrement? right)
    {
        return !(left == right);
    }
}
