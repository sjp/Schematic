using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization;

/// <summary>
/// Defines a serializer that writes and reads a database definition.
/// </summary>
public interface IRelationalDatabaseSerializer
{
    /// <summary>
    /// Writes a database definition to a stream.
    /// </summary>
    /// <param name="stream">The stream that the database definition will be written to.</param>
    /// <param name="database">The database to serialize.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes once the database definition has been written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="database"/> is <see langword="null"/>.</exception>
    Task SerializeAsync(Stream stream, IRelationalDatabase database, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a database definition from a stream.
    /// </summary>
    /// <param name="stream">A stream containing a serialized database definition.</param>
    /// <param name="identifierResolver">An identifier resolver used by the resulting database to look up objects.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The database described by the stream.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="identifierResolver"/> is <see langword="null"/>.</exception>
    Task<IRelationalDatabase> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default);
}
