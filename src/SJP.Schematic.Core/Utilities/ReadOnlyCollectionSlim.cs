using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SJP.Schematic.Core.Utilities
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public sealed class ReadOnlyCollectionSlim<T> : IReadOnlyCollection<T>
    {
        public ReadOnlyCollectionSlim(int count, IEnumerable<T> collection)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "The count must be non-negative");

            Count = count;
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public int Count { get; }

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();

        private readonly IEnumerable<T> _collection;
    }

    internal sealed class CollectionDebugView<T>
    {
        private readonly ICollection<T> _collection;

        public CollectionDebugView(ICollection<T> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
