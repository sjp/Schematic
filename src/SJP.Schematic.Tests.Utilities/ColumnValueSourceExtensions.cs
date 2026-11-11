using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// Convenience methods for testing the <see cref="ColumnValueSource"/> of a column.
/// </summary>
public static class ColumnValueSourceExtensions
{
    /// <summary>
    /// Unwraps a value source describing a computed column.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The computed case held by <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> does not describe a computed column.</exception>
    public static ColumnValueSource.Computed UnwrapComputed(this ColumnValueSource input)
    {
        return input.Value as ColumnValueSource.Computed
            ?? throw new ArgumentException("The given value source does not describe a computed column.", nameof(input));
    }

    /// <summary>
    /// Unwraps a value source describing a column generated as an identity, and returns its identity.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The identity of the column described by <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> does not describe a generated column.</exception>
    public static IAutoIncrement UnwrapIdentity(this ColumnValueSource input)
    {
        return input.Value is ColumnValueSource.Generated generated
            ? generated.Identity
            : throw new ArgumentException("The given value source does not describe a generated column.", nameof(input));
    }
}
