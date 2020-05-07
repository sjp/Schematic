using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// A dictionary that resolves its lookup identifiers before attempting to retrieve a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="IReadOnlyDictionary{TKey, TValue}" />
    public class IdentifierResolvingDictionary<TValue> : IReadOnlyDictionary<Identifier, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierResolvingDictionary{TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">A dictionary.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> or <paramref name="identifierResolver"/> is <c>null</c></exception>
        public IdentifierResolvingDictionary(IReadOnlyDictionary<Identifier, TValue> dictionary, IIdentifierResolutionStrategy identifierResolver)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            _identifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        /// <summary>
        /// Gets an enumerable collection that contains the identifiers that can be resolved to.
        /// </summary>
        public IEnumerable<Identifier> Keys => _dictionary.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values that are associated with a set of identifiers.
        /// </summary>
        public IEnumerable<TValue> Values => _dictionary.Values;

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> with the specified <see cref="Identifier"/>.
        /// </summary>
        /// <value>The <typeparamref name="TValue"/>.</value>
        /// <param name="key">The key.</param>
        /// <returns>A <typeparamref name="TValue"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no value is resolved for the given <paramref name="key"/>.</exception>
        public TValue this[Identifier key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                if (TryGetValue(key, out var value))
                    return value;

                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that can be resolved to by the given <see cref="Identifier"/>.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><see langword="true" /> if the read-only dictionary contains an element that can be resolved to by the given <see cref="Identifier"/>; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public bool ContainsKey(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var names = _identifierResolver.GetResolutionOrder(key);
            return names.Any(_dictionary.ContainsKey);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The <see cref="Identifier"/> to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified <see cref="Identifier"/>, if the <see cref="Identifier"/> can be resolved to; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true" /> if the lookup has an <see cref="Identifier"/> that can be resolved to; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public bool TryGetValue(Identifier key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var names = _identifierResolver.GetResolutionOrder(key);
            foreach (var name in names)
            {
                if (_dictionary.TryGetValue(name, out value))
                    return true;
            }

            value = default!;
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<Identifier, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        private readonly IReadOnlyDictionary<Identifier, TValue> _dictionary;
        private readonly IIdentifierResolutionStrategy _identifierResolver;
    }
}
