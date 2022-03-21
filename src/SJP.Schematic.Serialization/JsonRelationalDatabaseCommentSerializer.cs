using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Serialization.Mapping.Comments;

namespace SJP.Schematic.Serialization;

public class JsonRelationalDatabaseCommentSerializer : IRelationalDatabaseCommentSerializer
{
    public async Task SerializeAsync(Stream stream, IRelationalDatabaseCommentProvider databaseComments, CancellationToken cancellationToken = default)
    {
        var dbCommentMapper = new DatabaseCommentProviderMapper();
        var dto = await dbCommentMapper.MapAsync(databaseComments, cancellationToken).ConfigureAwait(false);
        await JsonSerializer.SerializeAsync(stream, dto, _settings.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IRelationalDatabaseCommentProvider> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        var dto = await JsonSerializer.DeserializeAsync<Dto.Comments.DatabaseCommentProvider>(stream, _settings.Value, cancellationToken).ConfigureAwait(false);
        if (dto == null)
            throw new InvalidOperationException("Unable to parse the given JSON as a database comment definition.");

        dto.IdentifierResolver = identifierResolver;

        var mapper = new DatabaseCommentProviderMapper();
        return mapper.Map(dto);
    }

    private static readonly Lazy<JsonSerializerOptions> _settings = new(LoadSettings);

    private static JsonSerializerOptions LoadSettings()
    {
        var settings = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        settings.Converters.Add(new JsonStringEnumConverter());

        return settings;
    }
}