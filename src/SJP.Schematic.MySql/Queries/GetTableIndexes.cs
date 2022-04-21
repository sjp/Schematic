namespace SJP.Schematic.MySql.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string IndexName { get; init; } = default!;

        public bool IsNonUnique { get; init; }

        public int ColumnOrdinal { get; init; }

        public string ColumnName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    index_name as `{ nameof(Result.IndexName) }`,
    non_unique as `{ nameof(Result.IsNonUnique) }`,
    seq_in_index as `{ nameof(Result.ColumnOrdinal) }`,
    column_name as `{ nameof(Result.ColumnName) }`
from information_schema.statistics
where table_schema = @{ nameof(Query.SchemaName) } and table_name = @{ nameof(Query.TableName) }";
}