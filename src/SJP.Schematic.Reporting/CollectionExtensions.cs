using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Reporting
{
    internal static class CollectionExtensions
    {
        public static uint UCount<T>(this IReadOnlyCollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var count = collection.Count;
            if (count < 0)
                throw new ArgumentException("The given collection has a negative count. This is not supported.", nameof(collection));

            return (uint)count;
        }

        public static uint UCount<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var count = collection.Count();
            if (count < 0)
                throw new ArgumentException("The given collection has a negative count. This is not supported.", nameof(collection));

            return (uint)count;
        }
    }
}
