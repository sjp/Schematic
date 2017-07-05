using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schema.Core.Utilities
{
    public class IdentifierLookup<TValue> : IReadOnlyDictionaryAsync<Identifier, TValue>
    {
        public IdentifierLookup(Func<Identifier, Task<TValue>> valueFactory, IdentifierComparer comparer = null, string defaultSchema = null)
        {
            if (comparer == null)
                comparer = IdentifierComparer.Ordinal;
            if (defaultSchema.IsNullOrWhiteSpace())
                _defaultSchema = null;

            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            _defaultSchema = defaultSchema;
            _store = new ConcurrentDictionary<Identifier, TValue>(comparer);
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
                TryGetValue(key, out var value);

                return value;
            }
        }

        public bool ContainsKey(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = EnsureValue(key).Result;

            return _store.ContainsKey(key);
        }

        public async Task<bool> ContainsKeyAsync(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = await EnsureValue(key);

            return _store.ContainsKey(key);
        }

        public bool TryGetValue(Identifier key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = EnsureValue(key).Result;

            return _store.TryGetValue(key, out value);
        }

        public async Task<IResult<TValue>> TryGetValueAsync(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = await EnsureValue(key);

            var success = _store.TryGetValue(key, out var value);
            return new Result<TValue>(success, value);
        }

        public IEnumerator<KeyValuePair<Identifier, TValue>> GetEnumerator() => _store.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();

        protected virtual async Task<TValue> EnsureValue(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_store.ContainsKey(key))
                return _store[key];

            var value = await _valueFactory.Invoke(key);
            _store.TryAdd(key, value);
            return value;
        }

        protected virtual Identifier CreateQualifiedName(Identifier source)
        {
            var localName = source.LocalName;
            var schemaName = source.Schema;
            if (_defaultSchema != null && schemaName.IsNullOrWhiteSpace())
                schemaName = _defaultSchema;

            return schemaName.IsNullOrWhiteSpace()
                ? new LocalIdentifier(localName)
                : new Identifier(schemaName, localName);
        }

        private static Func<Identifier, Task<TValue>> WrapFactoryAsAsync(Func<Identifier, TValue> valueFactory)
        {
            if (valueFactory == null)
                return null;

            return identifier => Task.FromResult(valueFactory.Invoke(identifier));
        }

        private readonly string _defaultSchema;
        private readonly ConcurrentDictionary<Identifier, TValue> _store;
        private readonly Func<Identifier, Task<TValue>> _valueFactory;
    }
}
