using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// A string comparer that ignores platform-specific line endings.
/// </summary>
public sealed class LineEndingInvariantStringComparer : IEqualityComparer<string>
{
    private readonly StringComparer _comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineEndingInvariantStringComparer"/> class.
    /// </summary>
    public LineEndingInvariantStringComparer()
        : this(StringComparison.Ordinal)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LineEndingInvariantStringComparer"/> class.
    /// </summary>
    /// <param name="comparison">The string comparison method.</param>
    public LineEndingInvariantStringComparer(StringComparison comparison)
    {
        _comparer = StringComparer.FromComparison(comparison);
    }

    /// <summary>
    /// A line-ending ignorant string comparer that uses the current culture for string comparison.
    /// </summary>
    /// <value>A line ending ignorant string comparer.</value>
    public static LineEndingInvariantStringComparer CurrentCulture { get; } = new LineEndingInvariantStringComparer(StringComparison.CurrentCulture);

    /// <summary>
    /// A line-ending ignorant string comparer that uses the current culture for string comparison, also ignoring case.
    /// </summary>
    /// <value>A line ending ignorant string comparer.</value>
    public static LineEndingInvariantStringComparer CurrentCultureIgnoreCase { get; } = new LineEndingInvariantStringComparer(StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    /// A line-ending ignorant string comparer that uses ordinal string comparison.
    /// </summary>
    /// <value>A line ending ignorant string comparer.</value>
    public static LineEndingInvariantStringComparer Ordinal { get; } = new LineEndingInvariantStringComparer(StringComparison.Ordinal);

    /// <summary>
    /// A line-ending ignorant string comparer that uses ordinal string comparison, also ignoring case.
    /// </summary>
    /// <value>A line ending ignorant string comparer.</value>
    public static LineEndingInvariantStringComparer OrdinalIgnoreCase { get; } = new LineEndingInvariantStringComparer(StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// A line-ending ignorant string comparer that uses the invariant culture for string comparison.
    /// </summary>
    /// <value>A line ending ignorant string comparer.</value>
    public static LineEndingInvariantStringComparer InvariantCulture { get; } = new LineEndingInvariantStringComparer(StringComparison.InvariantCulture);

    /// <summary>
    /// A line-ending ignorant string comparer that uses the invariant culture for string comparison, ignoring case.
    /// </summary>
    /// <value>A line ending ignorant string comparer.</value>
    public static LineEndingInvariantStringComparer InvariantCultureIgnoreCase { get; } = new LineEndingInvariantStringComparer(StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns><see langword="true" /> if the specified strings are equal; otherwise, <see langword="false" />.</returns>
    public bool Equals([AllowNull] string x, [AllowNull] string y)
    {
        if (x != null)
            x = x.ReplaceLineEndings();
        if (y != null)
            y = y.ReplaceLineEndings();

        return _comparer.Equals(x, y);
    }

    /// <summary>
    /// Returns a hash code for the given string.
    /// </summary>
    /// <param name="obj">A string.</param>
    /// <returns>A hash code for a string, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    /// <exception cref="ArgumentNullException">obj</exception>
    public int GetHashCode([DisallowNull] string obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return _comparer.GetHashCode(obj.ReplaceLineEndings());
    }
}