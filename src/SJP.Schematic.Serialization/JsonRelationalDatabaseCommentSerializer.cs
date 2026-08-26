using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Serialization.Mapping.Comments;

namespace SJP.Schematic.Serialization;

/// <summary>
/// Serializes the comments attached to a database's objects to and from JSON.
/// </summary>
public class JsonRelationalDatabaseCommentSerializer : IRelationalDatabaseCommentSerializer
{
    /// <summary>
    /// Writes a set of database comments to a stream as JSON.
    /// </summary>
    /// <param name="stream">The stream that the comments will be written to.</param>
    /// <param name="databaseComments">The database comments to serialize.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes once the comments have been written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="databaseComments"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Reads a set of database comments from a stream containing JSON.
    /// </summary>
    /// <param name="stream">A stream containing JSON database comments.</param>
    /// <param name="identifierResolver">An identifier resolver used by the resulting comment provider to look up objects.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The database comments described by the stream.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="identifierResolver"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The stream contains a JSON <c>null</c> literal instead of a comment definition.</exception>
    /// <exception cref="JsonException">The stream does not contain JSON that describes a comment definition.</exception>
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