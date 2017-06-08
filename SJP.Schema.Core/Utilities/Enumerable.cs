using System;
using System.Collections.Generic;
using System.Collections;

namespace SJP.Schema.Core.Utilities
{
    /// <summary>
    /// Provides a set of static methods for querying sequences.
    /// </summary>
    public static class EnumerableEx
    {
        /// <summary>
        /// Creates an enumerable that enumerates the original enumerable only once and caches its results.
        /// </summary>
        public static IEnumerable<TSource> MemoizeAll<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new MemoizeAllEnumerable<TSource>(source);
        }

        private class MemoizeAllEnumerable<T> : IEnumerable<T>
        {
            public MemoizeAllEnumerable(IEnumerable<T> source)
            {
                _source = source ?? throw new ArgumentNullException(nameof(source));
            }

            public IEnumerator<T> GetEnumerator()
            {
                return WalkList(() =>
                {
                    if (_source != null)
                    {
                        lock (_gate)
                        {
                            if (_source != null)
                            {
                                _list = LinkedList.Create(_source.GetEnumerator(), _gate);
                                _source = null;
                            }
                        }
                    }
                    return _list;
                });
            }

            private static IEnumerator<T> WalkList(Func<LinkedList> listFactory)
            {
                var current = listFactory();
                listFactory = null;
                while (current != null)
                {
                    yield return current.Current;
                    current = current.Next;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private LinkedList _list;
            private IEnumerable<T> _source;
            private readonly object _gate = new object();

            private class LinkedList
            {
                private LinkedList(IEnumerator<T> enumerator, T current, object gate)
                {
                    Current = current;
                    _enumerator = enumerator;
                    _gate = gate;
                }

                public static LinkedList Create(IEnumerator<T> enumerator, object gate)
                {
                    lock (gate)
                    {
                        if (!enumerator.MoveNext())
                        {
                            enumerator.Dispose();
                            return null;
                        }
                        return new LinkedList(enumerator, enumerator.Current, gate);
                    }
                }

                public LinkedList Next
                {
                    get
                    {
                        if (_enumerator != null)
                        {
                            lock (_gate)
                            {
                                if (_enumerator != null)
                                {
                                    _next = Create(_enumerator, _gate);
                                    _enumerator = null;
                                }
                            }
                        }
                        return _next;
                    }
                }

                public T Current { get; }

                private IEnumerator<T> _enumerator;
                private LinkedList _next;
                private readonly object _gate;
            }
        }

        /// <summary>
        /// Returns a sequence that invokes the enumerableFactory function whenever the sequence gets enumerated.
        /// </summary>
        public static IEnumerable<TSource> Defer<TSource>(Func<IEnumerable<TSource>> enumerableFactory)
        {
            if (enumerableFactory == null)
                throw new ArgumentNullException(nameof(enumerableFactory));

            return new DeferredEnumerable<TSource>(enumerableFactory);
        }

        private class DeferredEnumerable<T> : IEnumerable<T>
        {
            public DeferredEnumerable(Func<IEnumerable<T>> deferredEnumerable)
            {
                _enumerable = deferredEnumerable ?? throw new ArgumentNullException();
            }

            public IEnumerator<T> GetEnumerator() => _enumerable().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private readonly Func<IEnumerable<T>> _enumerable;
        }
    }
}