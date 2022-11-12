namespace SJP.Schematic.MySql.Queries;

internal static class GetTableUniqueKeys
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string ColumnName { get; init; }
    }

    internal const string Sql = @$"
select
    kc.constraint_name as `{nameof(Result.ConstraintName)}`,
    kc.column_name as `{nameof(Result.ColumnName)}`
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where t.table_schema = @{nameof(Query.SchemaName)} and t.table_name = @{nameof(Query.TableName)}
    and tc.constraint_type = 'UNIQUE'
order by kc.ordinal_position";
}