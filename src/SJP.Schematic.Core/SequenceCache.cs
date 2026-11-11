using System.ComponentModel;
using System.Diagnostics;

namespace SJP.Schematic.Core;

/// <summary>
/// Describes how a database keeps a supply of pre-allocated values for a sequence.
/// </summary>
/// <remarks>
/// <para>
/// Only a <see cref="Sized"/> cache carries a number, so the size of a cache can only be read
/// after switching on the cache.
/// </para>
/// <para>
/// Each case has value equality, so two caches that describe the same caching compare equal.
/// A <see langword="default"/> value holds no cache.
/// </para>
/// </remarks>
[DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
public readonly union SequenceCache(
    SequenceCache.Unknown,
    SequenceCache.None,
    SequenceCache.Sized,
    SequenceCache.EngineDefault
)
{
    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => Value?.ToString() ?? string.Empty;

    /// <summary>
    /// The database does not report how, or whether, the sequence's values are cached.
    /// </summary>
    public sealed record Unknown;

    /// <summary>
    /// No values are pre-allocated, i.e. <c>NO CACHE</c> in SQL Server or <c>NOCACHE</c> in Oracle.
    /// Every value is generated on demand.
    /// </summary>
    public sealed record None;

    /// <summary>
    /// A known number of values is pre-allocated.
    /// </summary>
    /// <param name="Size">The number of values that are pre-allocated, as the database reported it.</param>
    public sealed record Sized(int Size);

    /// <summary>
    /// Values are pre-allocated, but the database chooses how many and does not report the number,
    /// i.e. SQL Server's <c>CACHE</c> without a size.
    /// </summary>
    public sealed record EngineDefault;
}
