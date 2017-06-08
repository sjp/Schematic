using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SJP.Schema.Core.Utilities
{
    public class IdentifierLookup<TValue> : IReadOnlyDictionary<Identifier, TValue>
    {
        public IdentifierLookup(string defaultSchema, IReadOnlyDictionary<Identifier, TValue> store)
        {
            if (defaultSchema.IsNullOrWhiteSpace())
                _defaultSchema = null;

            _defaultSchema = defaultSchema;
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IEnumerable<Identifier> Keys => _store.Keys;

        public IEnumerable<TValue> Values => _store.Values;

        public int Count => _store.Count;

        public TValue this[Identifier key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                key = CreateQualifiedName(key);
                return _store[key];
            }
        }

        public bool ContainsKey(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            return _store.ContainsKey(key);
        }

        public bool TryGetValue(Identifier key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            return _store.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<Identifier, TValue>> GetEnumerator() => _store.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();

        private Identifier CreateQualifiedName(Identifier source)
        {
            var localName = source.LocalName;
            var schemaName = source.Schema;
            if (_defaultSchema != null && schemaName.IsNullOrWhiteSpace())
                schemaName = _defaultSchema;

            return schemaName.IsNullOrWhiteSpace()
                ? new LocalIdentifier(localName)
                : new Identifier(schemaName, localName);
        }

        private readonly string _defaultSchema;
        private readonly IReadOnlyDictionary<Identifier, TValue> _store;
    }

    public class LazyDictionaryCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        public LazyDictionaryCache(Func<TKey, TValue> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        // maybe change to Get(), GetAsync() methods?
        public TValue this[TKey key]
        {
            get
            {
                if (EqualityComparer<TKey>.Default.Equals(key, default(TKey)))
                    throw new ArgumentNullException(nameof(key));

                TValue value;
                _rwLock.EnterReadLock();
                try
                {
                    if (_store.TryGetValue(key, out value))
                        return value;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }

                _rwLock.EnterWriteLock();
                try
                {
                    value = _factory(key);
                    if (_store.TryAdd(key, value))
                        return value;
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                return value;
            }
        }

        public IEnumerable<TKey> Keys => _store.Keys;

        public IEnumerable<TValue> Values => _store.Values;

        public int Count => _store.Count;

        public bool ContainsKey(TKey key) => _store.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _store.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _store.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _rwLock.Dispose();

                _disposed = true;
            }
        }

        private bool _disposed;

        private readonly Func<TKey, TValue> _factory;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        private readonly ConcurrentDictionary<TKey, TValue> _store = new ConcurrentDictionary<TKey, TValue>();
    }
}
