namespace SJP.Schematic.MySql.Queries;

internal static class GetTableChildKeys
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ChildTableSchema { get; init; } = default!;

        public string ChildTableName { get; init; } = default!;

        public string ChildKeyName { get; init; } = default!;

        public string ParentKeyName { get; init; } = default!;

        public string ParentKeyType { get; init; } = default!;

        public string DeleteAction { get; init; } = default!;

        public string UpdateAction { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    t.table_schema as `{nameof(Result.ChildTableSchema)}`,
    t.table_name as `{nameof(Result.ChildTableName)}`,
    rc.constraint_name as `{nameof(Result.ChildKeyName)}`,
    rc.unique_constraint_name as `{nameof(Result.ParentKeyName)}`,
    ptc.constraint_type as `{nameof(Result.ParentKeyType)}`,
    rc.delete_rule as `{nameof(Result.DeleteAction)}`,
    rc.update_rule as `{nameof(Result.UpdateAction)}`
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where pt.table_schema = @{nameof(Query.SchemaName)} and pt.table_name = @{nameof(Query.TableName)}";
}