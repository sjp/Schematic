using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database schema provider that retrieves the schemas declared within a database.
/// </summary>
public interface IDatabaseSchemaProvider
{
    /// <summary>
    /// Enumerates all database schemas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database schemas.</returns>
    IAsyncEnumerable<IDatabaseSchema> EnumerateAllSchemas(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all database schemas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database schemas.</returns>
    Task<IReadOnlyCollection<IDatabaseSchema>> GetAllSchemas(CancellationToken cancellationToken = default);
}
