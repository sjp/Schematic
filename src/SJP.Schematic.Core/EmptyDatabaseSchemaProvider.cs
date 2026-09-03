using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database schema provider that returns no schemas. Not intended to be used directly.
/// </summary>
/// <seealso cref="IDatabaseSchemaProvider" />
public sealed class EmptyDatabaseSchemaProvider : IDatabaseSchemaProvider
{
    /// <summary>
    /// Enumerates all database schemas. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database schemas.</returns>
    public IAsyncEnumerable<IDatabaseSchema> EnumerateAllSchemas(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseSchema>();

    /// <summary>
    /// Gets all database schemas. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database schemas.</returns>
    public Task<IReadOnlyCollection<IDatabaseSchema>> GetAllSchemas(CancellationToken cancellationToken = default) => Empty.Tasks.Schemas;
}
