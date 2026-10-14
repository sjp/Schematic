using System;
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
    /// <returns><see langword="true" /> if the native <c>json</c> data type is supported; otherwise <see langword="false" />.</returns>
    public static bool SupportsJsonDataType(this IRelationalDatabaseProvider databaseProvider)
    {
        ArgumentNullException.ThrowIfNull(databaseProvider);

        return SqlServerIntegrationSetUp.DatabaseVersion >= MinJsonDataTypeVersion;
    }

    // Ordered clustered columnstore indexes were introduced in SQL Server 2022 (engine version 16.x).
    private static readonly Version MinOrderedColumnStoreIndexVersion = new(16, 0);

    /// <summary>
    /// Determines whether the connected SQL Server instance supports ordered clustered columnstore indexes.
    /// </summary>
    /// <param name="databaseProvider">A SQL Server database provider.</param>
    /// <returns><see langword="true" /> if ordered clustered columnstore indexes are supported; otherwise <see langword="false" />.</returns>
    public static bool SupportsOrderedColumnStoreIndex(this IRelationalDatabaseProvider databaseProvider)
    {
        ArgumentNullException.ThrowIfNull(databaseProvider);

        return SqlServerIntegrationSetUp.DatabaseVersion >= MinOrderedColumnStoreIndexVersion;
    }
}
