using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableStatistics
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result : ITableStatisticsRow
    {
        public required ulong? RowCount { get; init; }

        public required ulong? DataSizeBytes { get; init; }

        public required ulong? IndexSizeBytes { get; init; }
    }

    // See GetAllTableStatistics for what the information schema columns mean.
    internal const string Sql = $"""

select
    table_rows as `{nameof(Result.RowCount)}`,
    data_length as `{nameof(Result.DataSizeBytes)}`,
    index_length as `{nameof(Result.IndexSizeBytes)}`
from information_schema.tables
where table_schema = @{nameof(Query.SchemaName)}
    and table_name = @{nameof(Query.TableName)}
    and table_type = 'BASE TABLE'
""";
}
