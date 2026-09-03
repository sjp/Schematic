using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetAllTableStatistics
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }
    }

    internal sealed record Result : ITableStatisticsRow
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }

        public required ulong? RowCount { get; init; }

        public required ulong? DataSizeBytes { get; init; }

        public required ulong? IndexSizeBytes { get; init; }
    }

    // table_rows is an estimate for any engine that does not keep an exact count, InnoDB included,
    // where it is derived from sampled index pages and can be well out. It is null for a table
    // whose engine reports nothing, which is reported as an absent count rather than as zero rows.
    internal const string Sql = $"""

select
    table_schema as `{nameof(Result.SchemaName)}`,
    table_name as `{nameof(Result.TableName)}`,
    table_rows as `{nameof(Result.RowCount)}`,
    data_length as `{nameof(Result.DataSizeBytes)}`,
    index_length as `{nameof(Result.IndexSizeBytes)}`
from information_schema.tables
where table_schema = @{nameof(Query.SchemaName)}
    and table_type = 'BASE TABLE'
order by table_schema, table_name
""";
}
