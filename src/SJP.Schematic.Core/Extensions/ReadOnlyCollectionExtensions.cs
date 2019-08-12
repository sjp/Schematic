using System;
using System.Collections.Generic;

namespace SJP.Schematic.Core.Extensions
{
    public static class ReadOnlyCollectionExtensions
    {
        public static bool Empty<T>(this IReadOnlyCollection<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Count == 0;
        }
    }
}
