using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTableIndexDefinitions
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string IndexName { get; init; }

        public required string Sql { get; init; }
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"""

select
    name as "{nameof(Result.IndexName)}",
    sql as "{nameof(Result.Sql)}"
from {dialect.QuoteIdentifier(schemaName)}.sqlite_master
where type = 'index' and tbl_name = @{nameof(Query.TableName)} and sql is not null
""";
    }
}
