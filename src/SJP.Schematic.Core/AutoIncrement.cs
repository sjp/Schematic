using System;
using System.ComponentModel;

namespace SJP.Schematic.Core;

/// <summary>
/// A description of an auto-incrementing sequence.
/// </summary>
public readonly struct AutoIncrement : IAutoIncrement, IEquatable<AutoIncrement>, IEquatable<IAutoIncrement>
{
    /// <summary>
    /// Creates a description of an auto-incrementing sequence.
    /// </summary>
    /// <param name="initialValue">The starting value of the sequence.</param>
    /// <param name="increment">The value incremented to the current value on each new row.</param>
    /// <exception cref="ArgumentException"><paramref name="increment"/> is zero.</exception>
    public AutoIncrement(decimal initialValue, decimal increment)
    {
        InitialValue = initialValue;

        if (increment == 0)
            throw new ArgumentException("The increment value must be non-zero.", nameof(increment));

        Increment = increment;
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
    /// Indicates whether the current <see cref="AutoIncrement"/> object is equal to another <see cref="AutoIncrement"/> instance.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current <see cref="AutoIncrement"/> object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(AutoIncrement other)
    {
        return InitialValue == other.InitialValue
            && Increment == other.Increment;
    }

    /// <summary>
    /// Indicates whether the current <see cref="AutoIncrement"/> object is equal to an <see cref="IAutoIncrement"/> instance.
    /// </summary>
    /// <param name="other">An <see cref="IAutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the current <see cref="AutoIncrement"/> object is equal to <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(IAutoIncrement? other)
    {
        if (other == null)
            return false;

        return InitialValue == other.InitialValue
            && Increment == other.Increment;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object" />, is equal to this <see cref="AutoIncrement"/> instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns><see langword="true" /> if the specified <see cref="object" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        if (obj is AutoIncrement ai)
            return Equals(ai);
        if (obj is IAutoIncrement iai)
            return Equals(iai);

        return false;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => HashCode.Combine(InitialValue, Increment);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left <see cref="AutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="AutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(in AutoIncrement left, in AutoIncrement right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left <see cref="AutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="AutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is not equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(in AutoIncrement left, in AutoIncrement right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left <see cref="IAutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="AutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(IAutoIncrement left, in AutoIncrement right)
    {
        if (left == null)
            return false;

        return right.Equals(left);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left <see cref="IAutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="AutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is not equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(IAutoIncrement left, in AutoIncrement right)
    {
        if (left == null)
            return true;

        return !right.Equals(left);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left <see cref="AutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="IAutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(in AutoIncrement left, IAutoIncrement right)
    {
        if (right == null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left <see cref="AutoIncrement"/> instance.</param>
    /// <param name="right">The right <see cref="IAutoIncrement"/> instance.</param>
    /// <returns><see langword="true" /> if the <paramref name="left"/> object is not equal to the <paramref name="right"/> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(in AutoIncrement left, IAutoIncrement right)
    {
        if (right == null)
            return true;

        return !left.Equals(right);
    }
}