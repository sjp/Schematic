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
}
