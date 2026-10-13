using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests.Integration;

internal static class MySqlDialectExtensions
{
    /// <summary>
    /// Determines whether the connected server supports functional key parts, i.e. an index
    /// declared directly over an expression without a backing generated column. MySQL added this
    /// in 8.0.13; MariaDB has no equivalent syntax and requires a generated column to index an
    /// expression instead.
    /// </summary>
    /// <param name="databaseProvider">A MySQL database provider.</param>
    /// <returns><see langword="true" /> if functional key parts are supported; otherwise <see langword="false" />.</returns>
    public static bool SupportsFunctionalIndexes(this IRelationalDatabaseProvider databaseProvider)
    {
        ArgumentNullException.ThrowIfNull(databaseProvider);

        return !MySqlIntegrationSetUp.IsMariaDb;
    }

    /// <summary>
    /// Determines whether the connected server supports a <c>VECTOR</c> column type. MariaDB added
    /// it in 11.7 and MySQL in 9.0.
    /// </summary>
    /// <param name="databaseProvider">A MySQL database provider.</param>
    /// <returns><see langword="true" /> if vector columns are supported; otherwise <see langword="false" />.</returns>
    public static bool SupportsVectorType(this IRelationalDatabaseProvider databaseProvider)
    {
        ArgumentNullException.ThrowIfNull(databaseProvider);

        var minimumVersion = MySqlIntegrationSetUp.IsMariaDb
            ? new Version(11, 7)
            : new Version(9, 0);
        return MySqlIntegrationSetUp.ServerVersion >= minimumVersion;
    }
}
