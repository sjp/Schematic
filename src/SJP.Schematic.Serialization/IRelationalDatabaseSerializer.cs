using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization
{
    public interface IRelationalDatabaseSerializer
    {
        Task SerializeAsync(Stream stream, IRelationalDatabase database, CancellationToken cancellationToken = default);

        Task<IRelationalDatabase> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default);
    }
}
