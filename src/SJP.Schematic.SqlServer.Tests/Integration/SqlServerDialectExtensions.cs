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
    /// <param name="databaseProvider">A SQL Server database provider.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true" /> if the native <c>json</c> data type is supported; otherwise <see langword="false" />.</returns>
    public static async Task<bool> SupportsJsonDataType(this IRelationalDatabaseProvider databaseProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseProvider);

        var dbVersion = await databaseProvider.GetDatabaseVersionAsync(cancellationToken).ConfigureAwait(false);
        return dbVersion >= MinJsonDataTypeVersion;
    }
}
