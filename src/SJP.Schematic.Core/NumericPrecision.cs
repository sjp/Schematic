using System;
using System.ComponentModel;

namespace SJP.Schematic.Core;

/// <summary>
/// A numeric precision definition.
/// </summary>
/// <seealso cref="INumericPrecision" />
/// <seealso cref="IEquatable{NumericPrecision}" />
/// <seealso cref="IEquatable{INumericPrecision}" />
public readonly struct NumericPrecision : INumericPrecision, IEquatable<NumericPrecision>, IEquatable<INumericPrecision>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumericPrecision"/> struct.
    /// </summary>
    /// <param name="precision">The precision.</param>
    /// <param name="scale">The scale.</param>
    /// <exception cref="ArgumentException"><paramref name="precision"/> or <paramref name="scale"/> is negative.</exception>
    public NumericPrecision(int precision, int scale)
    {
        if (precision < 0)
            throw new ArgumentException("The precision must be non-negative.", nameof(precision));
        if (scale < 0)
            throw new ArgumentException("The precision must be non-negative.", nameof(scale));

        Precision = precision;
        Scale = scale;
    }

    /// <summary>
    /// The number of digits in a number.
    /// </summary>
    /// <value>A numeric value for the precision.</value>
    public int Precision { get; }

    /// <summary>
    /// The number of digits to the right of the decimal point in a number.
    /// </summary>
    /// <value>A numeric value for the scale.</value>
    public int Scale { get; }

    /// <summary>
    /// Determines whether the specified <see cref="object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        if (obj is NumericPrecision np)
            return Equals(np);
        if (obj is INumericPrecision inp)
            return Equals(inp);

        return false;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another numeric precision instance.
    /// </summary>
    /// <param name="other">A numeric precision instance to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(NumericPrecision other)
    {
        return Precision == other.Precision
            && Scale == other.Scale;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another numeric precision instance.
    /// </summary>
    /// <param name="other">A numeric precision instance to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(INumericPrecision? other)
    {
        if (other == null)
            return false;

        return Precision == other.Precision
            && Scale == other.Scale;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => HashCode.Combine(Precision, Scale);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns><see langword="true" /> if <paramref name="left"/> is equal to the <paramref name="right" /> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(in NumericPrecision left, in NumericPrecision right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns><see langword="true" /> if <paramref name="left"/> is not equal to the <paramref name="right" /> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(in NumericPrecision left, in NumericPrecision right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns><see langword="true" /> if <paramref name="left"/> is equal to the <paramref name="right" /> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(INumericPrecision left, in NumericPrecision right)
    {
        if (left == null)
            return false;

        return right.Equals(left);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns><see langword="true" /> if <paramref name="left"/> is not equal to the <paramref name="right" /> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(INumericPrecision left, in NumericPrecision right)
    {
        if (left == null)
            return true;

        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns><see langword="true" /> if <paramref name="left"/> is equal to the <paramref name="right" /> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(in NumericPrecision left, INumericPrecision right)
    {
        if (right == null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns><see langword="true" /> if <paramref name="left"/> is not equal to the <paramref name="right" /> parameter; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(in NumericPrecision left, INumericPrecision right)
    {
        if (right == null)
            return true;

        return !(left == right);
    }
}
