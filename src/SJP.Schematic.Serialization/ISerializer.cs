using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Serialization
{
    public interface ISerializer<T, TOut>
    {
        Task<TOut> SerializeAsync(T obj, CancellationToken cancellationToken = default);

        Task<T> DeserializeAsync(TOut input, CancellationToken cancellationToken = default);
    }
}
