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

    internal const string Sql = $"""

select
    rc.unique_constraint_schema as `{nameof(Result.ParentTableSchema)}`,
    rc.referenced_table_name as `{nameof(Result.ParentTableName)}`,
    rc.constraint_name as `{nameof(Result.ChildKeyName)}`,
    rc.unique_constraint_name as `{nameof(Result.ParentKeyName)}`,
    kc.column_name as `{nameof(Result.ColumnName)}`,
    kc.ordinal_position as `{nameof(Result.ConstraintColumnId)}`,
    ptc.constraint_type as `{nameof(Result.ParentKeyType)}`,
    rc.delete_rule as `{nameof(Result.DeleteAction)}`,
    rc.update_rule as `{nameof(Result.UpdateAction)}`
from information_schema.referential_constraints rc
inner join information_schema.key_column_usage kc
    on kc.constraint_schema = rc.constraint_schema
    and kc.constraint_name = rc.constraint_name
    and kc.table_schema = rc.constraint_schema
    and kc.table_name = rc.table_name
inner join information_schema.table_constraints ptc
    on ptc.table_schema = rc.unique_constraint_schema
    and ptc.table_name = rc.referenced_table_name
    and ptc.constraint_name = rc.unique_constraint_name
where rc.constraint_schema = @{nameof(Query.SchemaName)} and rc.table_name = @{nameof(Query.TableName)}
order by rc.constraint_name, kc.ordinal_position
""";
}