using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization;

public interface IRelationalDatabaseSerializer
{
    Task SerializeAsync(Stream stream, IRelationalDatabase database, CancellationToken cancellationToken = default);

    Task<IRelationalDatabase> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default);
}

public interface IRelationalDatabaseCommentSerializer
{
    Task SerializeAsync(Stream stream, IRelationalDatabaseCommentProvider databaseComments, CancellationToken cancellationToken = default);

    Task<IRelationalDatabaseCommentProvider> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default);
}