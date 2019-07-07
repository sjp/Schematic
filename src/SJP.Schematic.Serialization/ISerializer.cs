using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Serialization
{
    public interface ISerializer<T, TOut>
    {
        TOut Serialize(T obj);

        Task<TOut> SerializeAsync(T obj, CancellationToken cancellationToken = default);

        T Deserialize(TOut input);

        Task<T> DeserializeAsync(TOut input, CancellationToken cancellationToken = default);
    }
}
