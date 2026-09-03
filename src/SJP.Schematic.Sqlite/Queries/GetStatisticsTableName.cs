using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetStatisticsTableName
{
    // SQLite creates sqlite_stat1 only when ANALYZE has been run, and a database attached to the
    // same connection may or may not have one, so each schema is asked separately.
    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"""

select name
from {dialect.QuoteIdentifier(schemaName)}.sqlite_master
where type = 'table' and name = 'sqlite_stat1'
limit 1
""";
    }
}
