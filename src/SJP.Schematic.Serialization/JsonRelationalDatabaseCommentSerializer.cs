using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Serialization.Mapping.Comments;

namespace SJP.Schematic.Serialization;

public class JsonRelationalDatabaseCommentSerializer : IRelationalDatabaseCommentSerializer
{
    public Task SerializeAsync(Stream stream, IRelationalDatabaseCommentProvider databaseComments, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(databaseComments);

        return SerializeAsyncCore(stream, databaseComments, cancellationToken);
    }

    private static async Task SerializeAsyncCore(Stream stream, IRelationalDatabaseCommentProvider databaseComments, CancellationToken cancellationToken)
    {
        var dto = await _mapper.MapAsync(databaseComments, cancellationToken);
        await JsonSerializer.SerializeAsync(stream, dto, JsonSerializerSettings.Default, cancellationToken);
    }

    public Task<IRelationalDatabaseCommentProvider> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        return DeserializeAsyncCore(stream, identifierResolver, cancellationToken);
    }

    private static async Task<IRelationalDatabaseCommentProvider> DeserializeAsyncCore(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
    {
        var dto = await JsonSerializer.DeserializeAsync<Dto.Comments.DatabaseCommentProvider>(stream, JsonSerializerSettings.Default, cancellationToken);
        if (dto == null)
            throw new InvalidOperationException("Unable to parse the given JSON as a database comment definition.");

        return _mapper.Map(dto, identifierResolver);
    }

    private static readonly DatabaseCommentProviderMapper _mapper = new();
}