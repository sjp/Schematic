using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal static class OracleDialectExtensions
{
    // The native JSON data type was introduced in Oracle Database 21c.
    private static readonly Version MinJsonDataTypeVersion = new(21, 0);

    /// <summary>
    /// Determines whether the connected Oracle instance supports the native <c>JSON</c> data type.
    /// </summary>
    /// <param name="databaseProvider">An Oracle database provider.</param>
    /// <returns><see langword="true" /> if the native <c>JSON</c> data type is supported; otherwise <see langword="false" />.</returns>
    public static bool SupportsJsonDataType(this IRelationalDatabaseProvider databaseProvider)
    {
        ArgumentNullException.ThrowIfNull(databaseProvider);

        return OracleIntegrationSetUp.DatabaseVersion >= MinJsonDataTypeVersion;
    }
}
