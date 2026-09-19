using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableParentKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result : IForeignKeyColumnRow
    {
        public required string ParentTableSchema { get; init; }

        public required string ParentTableName { get; init; }

        public required string ChildKeyName { get; init; }

        public required string ParentKeyName { get; init; }

        public required string ColumnName { get; init; }

        public required string ParentKeyType { get; init; }

        public required string DeleteAction { get; init; }

        public required string UpdateAction { get; init; }
    }

    // MariaDB fills information_schema tables by opening each table unless the schema and table name are
    // constants, so key_column_usage is filtered on the table itself instead of through the join. A primary
    // key is always named PRIMARY and no other index may use that name, so the parent key type comes from the
    // referenced constraint's name. Joining table_constraints on the parent table would open every table.
    internal const string Sql = $"""

select
    rc.unique_constraint_schema as `{nameof(Result.ParentTableSchema)}`,
    rc.referenced_table_name as `{nameof(Result.ParentTableName)}`,
    rc.constraint_name as `{nameof(Result.ChildKeyName)}`,
    rc.unique_constraint_name as `{nameof(Result.ParentKeyName)}`,
    kc.column_name as `{nameof(Result.ColumnName)}`,
    case when rc.unique_constraint_name = 'PRIMARY' then 'PRIMARY KEY' else 'UNIQUE' end as `{nameof(Result.ParentKeyType)}`,
    rc.delete_rule as `{nameof(Result.DeleteAction)}`,
    rc.update_rule as `{nameof(Result.UpdateAction)}`
from information_schema.referential_constraints rc
inner join information_schema.key_column_usage kc
    on kc.table_schema = @{nameof(Query.SchemaName)}
    and kc.table_name = @{nameof(Query.TableName)}
    and kc.constraint_schema = rc.constraint_schema
    and kc.constraint_name = rc.constraint_name
where rc.constraint_schema = @{nameof(Query.SchemaName)} and rc.table_name = @{nameof(Query.TableName)}
    and rc.unique_constraint_name is not null
order by rc.constraint_name, kc.ordinal_position
""";
}