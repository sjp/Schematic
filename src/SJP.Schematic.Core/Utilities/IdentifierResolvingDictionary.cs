using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Utilities
{
    public class IdentifierResolvingDictionary<TValue> : IReadOnlyDictionary<Identifier, TValue>
    {
        public IdentifierResolvingDictionary(IReadOnlyDictionary<Identifier, TValue> dictionary, IIdentifierResolutionStrategy identifierResolver)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            _identifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        public IEnumerable<Identifier> Keys => _dictionary.Keys;

        public IEnumerable<TValue> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

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

        public bool ContainsKey(Identifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var names = _identifierResolver.GetResolutionOrder(key);
            return names.Any(_dictionary.ContainsKey);
        }

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

            value = default(TValue);
            return false;
        }

        public IEnumerator<KeyValuePair<Identifier, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        private readonly IReadOnlyDictionary<Identifier, TValue> _dictionary;
        private readonly IIdentifierResolutionStrategy _identifierResolver;
    }
}
