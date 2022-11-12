namespace SJP.Schematic.MySql.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string IndexName { get; init; }

        public required bool IsNonUnique { get; init; }

        public required int ColumnOrdinal { get; init; }

        public required string ColumnName { get; init; }
    }

    internal const string Sql = @$"
select
    index_name as `{nameof(Result.IndexName)}`,
    non_unique as `{nameof(Result.IsNonUnique)}`,
    seq_in_index as `{nameof(Result.ColumnOrdinal)}`,
    column_name as `{nameof(Result.ColumnName)}`
from information_schema.statistics
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.TableName)}";
}