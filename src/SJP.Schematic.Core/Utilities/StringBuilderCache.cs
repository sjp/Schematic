using System;
using System.Text;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// For internal use only. A cache of <see cref="StringBuilder"/> instances. Reduces allocation across Schematic operations.
/// </summary>
/// <remarks>This is largely taken from Roslyn and netfx internals.</remarks>
public static class StringBuilderCache
{
    // The value 360 was chosen in discussion with performance experts as a compromise between using
    // as little memory (per thread) as possible and still covering a large part of short-lived
    // StringBuilder creations on the startup path of VS designers.
    private const int MaxBuilderSize = 360;
    private const int DefaultCapacity = 16;

    [ThreadStatic]
    private static StringBuilder? _cachedInstance;

    /// <summary>
    /// Acquires a <see cref="StringBuilder"/> instance.
    /// </summary>
    /// <param name="capacity">The capacity that should be pre-allocated when a <see cref="StringBuilder"/> is available.</param>
    /// <returns>A <see cref="StringBuilder"/> instance.</returns>
    public static StringBuilder Acquire(int capacity = DefaultCapacity)
    {
        if (capacity <= MaxBuilderSize)
        {
            var builder = _cachedInstance;
            // Avoid StringBuilder block fragmentation by getting a new StringBuilder
            // when the requested size is larger than the current capacity
            if (builder != null && capacity <= builder.Capacity)
            {
                _cachedInstance = null;
                builder.Clear();
                return builder;
            }
        }
        return new StringBuilder(capacity);
    }

    /// <summary>
    /// Allocates a <see cref="string"/> and returns the <see cref="StringBuilder"/> instance to the pool.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance.</param>
    /// <returns>The <see cref="string"/> contents of the <see cref="StringBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <c>null</c>.</exception>
    public static string GetStringAndRelease(this StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var result = builder.ToString();
        Release(builder);
        return result;
    }

    private static void Release(StringBuilder builder)
    {
        if (builder.Capacity <= MaxBuilderSize)
            _cachedInstance = builder;
    }
}