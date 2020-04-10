using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization
{
    public interface IRelationalDatabaseSerializer : ISerializer<IRelationalDatabase, string>
    {
        Task<IRelationalDatabase> DeserializeAsync(string input, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default);
    }
}
