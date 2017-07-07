using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schema.Core.Utilities
{
    public interface ICache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        IEnumerable<TKey> Keys { get; }

        IEnumerable<TValue> Values { get; }

        bool ContainsKey(TKey key);

        Task<bool> ContainsKeyAsync(TKey key);

        TValue GetValue(TKey key);

        Task<TValue> GetValueAsync(TKey key);

        void Remove(TKey key);

        void Clear();
    }
}
