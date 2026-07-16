using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Additional functionality provided by a SQL Server database provider.
/// </summary>
public interface ISqlServerDatabaseProvider : IRelationalDatabaseProvider
{
    /// <summary>
    /// Retrieve the assigned compatibility level for the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A representation of the compatibility level assigned to the database.</returns>
    Task<CompatibilityLevel> GetCompatibilityLevel(CancellationToken cancellationToken = default);
}
