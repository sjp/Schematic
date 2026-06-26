using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal static class SqlServerDialectExtensions
{
    // The native JSON data type was introduced in SQL Server 2025 (engine version 17.x).
    private static readonly Version MinJsonDataTypeVersion = new(17, 0);

    /// <summary>
    /// Determines whether the connected SQL Server instance supports the native <c>json</c> data type.
    /// </summary>
    /// <param name="dialect">A SQL Server dialect.</param>
    /// <param name="connection">A connection to a SQL Server database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true" /> if the native <c>json</c> data type is supported; otherwise <see langword="false" />.</returns>
    public static async Task<bool> SupportsJsonDataType(this ISqlServerDialect dialect, ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dialect);

        var dbVersion = await dialect.GetDatabaseVersionAsync(connection, cancellationToken).ConfigureAwait(false);
        return dbVersion >= MinJsonDataTypeVersion;
    }
}
