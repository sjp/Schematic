namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTablePrimaryKey
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public int OrdinalPosition { get; init; }
    }

    internal const string Sql = @$"
select
    kc.constraint_name as ""{ nameof(Result.ConstraintName) }"",
    kc.column_name as ""{ nameof(Result.ColumnName) }"",
    kc.ordinal_position as ""{ nameof(Result.OrdinalPosition) }""
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @{ nameof(Query.SchemaName) } and tc.table_name = @{ nameof(Query.TableName) }
    and tc.constraint_type = 'PRIMARY KEY'";
}