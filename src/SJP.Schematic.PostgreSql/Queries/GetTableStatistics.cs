using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableStatistics
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result : ITableStatisticsRow
    {
        public required long RowCount { get; init; }

        public required long? DataSizeBytes { get; init; }

        public required long? IndexSizeBytes { get; init; }
    }

    // See GetAllTableStatistics for what the catalog columns mean.
    internal const string Sql = $"""

select
    cast(t.reltuples as bigint) as "{nameof(Result.RowCount)}",
    pg_catalog.pg_table_size(t.oid) as "{nameof(Result.DataSizeBytes)}",
    pg_catalog.pg_indexes_size(t.oid) as "{nameof(Result.IndexSizeBytes)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
where t.relkind in ('r', 'p')
    and ns.nspname = @{nameof(Query.SchemaName)}
    and t.relname = @{nameof(Query.TableName)}
""";
}
