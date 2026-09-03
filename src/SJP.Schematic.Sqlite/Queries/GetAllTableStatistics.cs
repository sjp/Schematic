using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetAllTableStatistics
{
    internal sealed record Result
    {
        public required string TableName { get; init; }

        /// <summary>The index the row describes, or <see langword="null" /> for the table itself.</summary>
        public required string? IndexName { get; init; }

        /// <summary>A space-separated list of estimates whose first entry is the number of rows.</summary>
        public required string? Stat { get; init; }
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"""

select
    tbl as "{nameof(Result.TableName)}",
    idx as "{nameof(Result.IndexName)}",
    stat as "{nameof(Result.Stat)}"
from {dialect.QuoteIdentifier(schemaName)}.sqlite_stat1
order by tbl
""";
    }
}
