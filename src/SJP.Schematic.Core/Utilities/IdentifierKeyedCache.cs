using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Utilities
{
    public class IdentifierKeyedCache<TValue> : ICache<Identifier, TValue> where TValue : class
    {
        public IdentifierKeyedCache(Func<Identifier, Task<TValue>> valueFactory, IEqualityComparer<Identifier> comparer = null, string defaultSchema = null)
        {
            if (comparer == null)
                comparer = IdentifierComparer.Ordinal;
            if (defaultSchema.IsNullOrWhiteSpace())
                _defaultSchema = null;

            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            _defaultSchema = defaultSchema;
            _store = new ConcurrentDictionary<Identifier, AsyncLazy<TValue>>(comparer);
        }

        public IEnumerable<Identifier> Keys => NotNullValues.Select(kv => kv.Key);

        public IEnumerable<TValue> Values => NotNullValues.Select(kv => kv.Value);

        public bool ContainsKey(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = EnsureValue(key).Result;
            return tmp != null;
        }

        public async Task<bool> ContainsKeyAsync(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = await GetValueAsync(key).ConfigureAwait(false);
            return tmp != null;
        }

        public TValue GetValue(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = EnsureValue(key);

            _store.TryGetValue(key, out var lazy);
            return lazy.Task.Result;
        }

        public Task<TValue> GetValueAsync(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = EnsureValue(key);

            _store.TryGetValue(key, out var lazy);
            return lazy.Task;
        }

        protected virtual Task<TValue> EnsureValue(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_store.ContainsKey(key))
                return _store[key].Task;

            var lazy = new AsyncLazy<TValue>(() => _valueFactory.Invoke(key));
            _store.TryAdd(key, lazy);
            return _store[key].Task;
        }

        protected virtual Identifier CreateQualifiedName(Identifier source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var localName = source.LocalName;
            var schemaName = source.Schema;
            if (_defaultSchema != null && schemaName.IsNullOrWhiteSpace())
                schemaName = _defaultSchema;

            return schemaName.IsNullOrWhiteSpace()
                ? new LocalIdentifier(localName)
                : new Identifier(schemaName, localName);
        }

        public void Remove(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _store.TryRemove(key, out var value);
        }

        public void Clear() => _store.Clear();

        public IEnumerator<KeyValuePair<Identifier, TValue>> GetEnumerator() => NotNullValues.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected IEnumerable<KeyValuePair<Identifier, TValue>> NotNullValues => _store
            .Select(kv => new KeyValuePair<Identifier, TValue>(kv.Key, kv.Value.Task.Result))
            .Where(kv => kv.Value != null);

        private readonly string _defaultSchema;
        private readonly ConcurrentDictionary<Identifier, AsyncLazy<TValue>> _store;
        private readonly Func<Identifier, Task<TValue>> _valueFactory;
    }
}
