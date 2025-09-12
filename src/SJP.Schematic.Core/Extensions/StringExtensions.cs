using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods for working with <see cref="string"/> objects.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Indicates whether the specified string is <see langword="null" /> or an empty string (<c>""</c>).
    /// </summary>
    /// <param name="input">The string to test.</param>
    /// <returns><see langword="true" /> if <paramref name="input"/> is <see langword="null" /> or an empty string (<c>""</c>); otherwise, <see langword="false" />.</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? input) => string.IsNullOrEmpty(input);

    /// <summary>
    /// Indicates whether the specified string is <see langword="null" /> or empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="input">The string to test.</param>
    /// <returns><see langword="true" /> if <paramref name="input"/> is <see langword="null" /> or an empty string (<c>""</c>); or if <paramref name="input"/> consists exclusively of white-space characters otherwise, <see langword="false" />.</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? input) => string.IsNullOrWhiteSpace(input);

    /// <summary>
    /// Concatenates the members of a constructed <see cref="IEnumerable{String}"/>, using the specified separator between each member.
    /// </summary>
    /// <param name="values">A collection that contains the strings to concatenate.</param>
    /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the returned string only if values has more than one element.</param>
    /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string. Alternatively <c>""</c> if <paramref name="values"/> has zero elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> or <paramref name="separator"/> is <see langword="null" /></exception>
    public static string Join(this IEnumerable<string> values, string separator)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(separator);

        return string.Join(separator, values);
    }
}