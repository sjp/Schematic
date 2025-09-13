using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Additional functionality provided by a SQL Server dialect.
/// </summary>
public interface ISqlServerDialect : IDatabaseDialect
{
    /// <summary>
    /// Retrieve the assigned compatibility level for a given database.
    /// </summary>
    /// <param name="connection">A connection to a SQL Server database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A representation of the compatibility level assigned to the database.</returns>
    Task<CompatibilityLevel> GetCompatibilityLevel(ISchematicConnection connection, CancellationToken cancellationToken = default);
}
