using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;

namespace SJP.Schematic.Serialization;

public class JsonRelationalDatabaseSerializer : IRelationalDatabaseSerializer
{
    public Task SerializeAsync(Stream stream, IRelationalDatabase database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(database);

        return SerializeAsyncCore(stream, database, cancellationToken);
    }

    private static async Task SerializeAsyncCore(Stream stream, IRelationalDatabase database, CancellationToken cancellationToken)
    {
        var dto = await _mapper.MapAsync(database, cancellationToken);
        await JsonSerializer.SerializeAsync(stream, dto, JsonSerializerSettings.Default, cancellationToken);
    }

    public Task<IRelationalDatabase> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        return DeserializeAsyncCore(stream, identifierResolver, cancellationToken);
    }

    private static async Task<IRelationalDatabase> DeserializeAsyncCore(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
    {
        var dto = await JsonSerializer.DeserializeAsync<Dto.RelationalDatabase>(stream, JsonSerializerSettings.Default, cancellationToken);
        if (dto == null)
            throw new InvalidOperationException("Unable to parse the given JSON as a database definition.");

        return _mapper.Map(dto, identifierResolver);
    }

    private static readonly RelationalDatabaseMapper _mapper = new();
}