using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.ToList().AsReadOnly();
        }
    }
}
