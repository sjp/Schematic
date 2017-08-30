using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// A cache for storing database values whose keys are database identifiers.
    /// </summary>
    /// <typeparam name="TValue">The type of database related object to cache.</typeparam>
    public class IdentifierKeyedCache<TValue> : ICache<Identifier, TValue> where TValue : class
    {
        /// <summary>
        /// Initialises a new IdentifierKeyedCache store.
        /// </summary>
        /// <param name="valueFactory">The function to use to generate new values lazily.</param>
        /// <param name="comparer">The comparer to use to determine whether two identifier keys are equal.</param>
        /// <param name="defaultSchema">The default schema of the database, used to avoid duplicating keys when unnecessary.</param>
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

        /// <summary>
        /// Gets a collection of keys stored by the cache.
        /// </summary>
        public IEnumerable<Identifier> Keys => NotNullValues.Select(kv => kv.Key);

        /// <summary>
        /// Gets a collection of values stored by the cache.
        /// </summary>
        public IEnumerable<TValue> Values => NotNullValues.Select(kv => kv.Value);

        /// <summary>
        /// Determines whether the cache contains a given identifier.
        /// </summary>
        /// <param name="key">The identifier to locate in the cache.</param>
        /// <returns><c>true</c> if the cache contains the identifier, <c>false</c> otherwise.</returns>
        public bool ContainsKey(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = EnsureValue(key).Result;
            return tmp != null;
        }

        /// <summary>
        /// Asynchronously determines whether the cache contains a given identifier.
        /// </summary>
        /// <param name="key">The identifier to locate in the cache.</param>
        /// <returns><c>true</c> if the cache contains the identifier, <c>false</c> otherwise.</returns>
        public async Task<bool> ContainsKeyAsync(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = await GetValueAsync(key).ConfigureAwait(false);
            return tmp != null;
        }

        /// <summary>
        /// Retrieves the value from the cache that is associated with a specific identifier.
        /// </summary>
        /// <param name="key">The identifier of the value to retrieve.</param>
        /// <returns>A value stored in the cache.</returns>
        public TValue GetValue(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = EnsureValue(key);

            _store.TryGetValue(key, out var lazy);
            return lazy.Task.Result;
        }

        /// <summary>
        /// Asynchronously retrieves the value from the cache that is associated with a specific identifier.
        /// </summary>
        /// <param name="key">The identifier of the value to retrieve.</param>
        /// <returns>A value stored in the cache.</returns>
        public Task<TValue> GetValueAsync(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = CreateQualifiedName(key);
            var tmp = EnsureValue(key);

            _store.TryGetValue(key, out var lazy);
            return lazy.Task;
        }

        /// <summary>
        /// Evaluates the factory method to ensure that a value (null or not) is present in the cache.
        /// </summary>
        /// <param name="key">The identifier of the value to retrieve.</param>
        /// <returns>A value to be stored in the cache.</returns>
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

        /// <summary>
        /// A convenience method to ensure that names are qualified by schema to avoid ambiguity.
        /// </summary>
        /// <param name="key">The identifier to qualify by schema (if missing).</param>
        /// <returns>A schema-qualified identifier.</returns>
        protected virtual Identifier CreateQualifiedName(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var localName = key.LocalName;
            var schemaName = key.Schema;
            if (_defaultSchema != null && schemaName.IsNullOrWhiteSpace())
                schemaName = _defaultSchema;

            return schemaName.IsNullOrWhiteSpace()
                ? new LocalIdentifier(localName)
                : new Identifier(schemaName, localName);
        }

        /// <summary>
        /// Clears the cache of any values that it is currently storing.
        /// </summary>
        public void Clear() => _store.Clear();

        /// <summary>
        /// An enumerator for the values stored in the cache. Does not enumerate values known to be missing.
        /// </summary>
        /// <returns>An enumerator for cached values.</returns>
        public IEnumerator<KeyValuePair<Identifier, TValue>> GetEnumerator() => NotNullValues.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns a collection of non-null values that have been stored in the cache.
        /// </summary>
        protected IEnumerable<KeyValuePair<Identifier, TValue>> NotNullValues => _store
            .Select(kv => new KeyValuePair<Identifier, TValue>(kv.Key, kv.Value.Task.Result))
            .Where(kv => kv.Value != null);

        private readonly string _defaultSchema;
        private readonly ConcurrentDictionary<Identifier, AsyncLazy<TValue>> _store;
        private readonly Func<Identifier, Task<TValue>> _valueFactory;
    }
}
