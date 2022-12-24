using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableParentKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ParentTableSchema { get; init; }

        public required string ParentTableName { get; init; }

        public required string ChildKeyName { get; init; }

        public required string ParentKeyName { get; init; }

        public required string ColumnName { get; init; }

        public required int ConstraintColumnId { get; init; }

        public required string ParentKeyType { get; init; }

        public required string DeleteAction { get; init; }

        public required string UpdateAction { get; init; }
    }

    internal const string Sql = @$"
select
    pt.table_schema as `{nameof(Result.ParentTableSchema)}`,
    pt.table_name as `{nameof(Result.ParentTableName)}`,
    rc.constraint_name as `{nameof(Result.ChildKeyName)}`,
    rc.unique_constraint_name as `{nameof(Result.ParentKeyName)}`,
    kc.column_name as `{nameof(Result.ColumnName)}`,
    kc.ordinal_position as `{nameof(Result.ConstraintColumnId)}`,
    ptc.constraint_type as `{nameof(Result.ParentKeyType)}`,
    rc.delete_rule as `{nameof(Result.DeleteAction)}`,
    rc.update_rule as `{nameof(Result.UpdateAction)}`
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where t.table_schema = @{nameof(Query.SchemaName)} and t.table_name = @{nameof(Query.TableName)}";
}