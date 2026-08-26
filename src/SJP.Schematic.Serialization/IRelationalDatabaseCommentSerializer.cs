using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization;

/// <summary>
/// Defines a serializer that writes and reads the comments attached to a database's objects.
/// </summary>
public interface IRelationalDatabaseCommentSerializer
{
    /// <summary>
    /// Writes a set of database comments to a stream.
    /// </summary>
    /// <param name="stream">The stream that the comments will be written to.</param>
    /// <param name="databaseComments">The database comments to serialize.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes once the comments have been written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="databaseComments"/> is <see langword="null"/>.</exception>
    Task SerializeAsync(Stream stream, IRelationalDatabaseCommentProvider databaseComments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a set of database comments from a stream.
    /// </summary>
    /// <param name="stream">A stream containing serialized database comments.</param>
    /// <param name="identifierResolver">An identifier resolver used by the resulting comment provider to look up objects.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The database comments described by the stream.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="identifierResolver"/> is <see langword="null"/>.</exception>
    Task<IRelationalDatabaseCommentProvider> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default);
}
