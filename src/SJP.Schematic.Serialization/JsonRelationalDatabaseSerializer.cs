using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;

namespace SJP.Schematic.Serialization;

/// <summary>
/// Serializes a database definition to and from JSON.
/// </summary>
public class JsonRelationalDatabaseSerializer : IRelationalDatabaseSerializer
{
    /// <summary>
    /// Writes a database definition to a stream as JSON.
    /// </summary>
    /// <param name="stream">The stream that the database definition will be written to.</param>
    /// <param name="database">The database to serialize.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes once the database definition has been written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="database"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Reads a database definition from a stream containing JSON.
    /// </summary>
    /// <param name="stream">A stream containing a JSON database definition.</param>
    /// <param name="identifierResolver">An identifier resolver used by the resulting database to look up objects.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The database described by the stream.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="identifierResolver"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The stream contains a JSON <c>null</c> literal instead of a database definition.</exception>
    /// <exception cref="JsonException">The stream does not contain JSON that describes a database definition.</exception>
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