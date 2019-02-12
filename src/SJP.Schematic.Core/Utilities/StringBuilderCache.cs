using System;
using System.Text;

namespace SJP.Schematic.Core.Utilities
{
    public static class StringBuilderCache
    {
        // The value 360 was chosen in discussion with performance experts as a compromise between using
        // as little memory (per thread) as possible and still covering a large part of short-lived
        // StringBuilder creations on the startup path of VS designers.
        private const int MaxBuilderSize = 360;
        private const int DefaultCapacity = 16;

        [ThreadStatic]
        private static StringBuilder _cachedInstance;

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

        public static string GetStringAndRelease(this StringBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

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
}
