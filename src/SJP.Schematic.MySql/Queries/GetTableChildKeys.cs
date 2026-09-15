using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableChildKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ChildTableSchema { get; init; }

        public required string ChildTableName { get; init; }

        public required string ChildKeyName { get; init; }

        public required string ParentKeyName { get; init; }

        public required string ParentKeyType { get; init; }

        public required string DeleteAction { get; init; }

        public required string UpdateAction { get; init; }
    }

    // A primary key is always named PRIMARY, and no other index may use that name, so the parent key type
    // is read from the referenced constraint's name rather than by joining table_constraints. On MariaDB the
    // join could not use a lookup key and scanned every table on the server a second time.
    //
    // Child tables can be in any schema, so referential_constraints is filtered on the referenced table only.
    // MariaDB cannot look that up by key and still reads every table on the server to answer it.
    internal const string Sql = $"""

select
    rc.constraint_schema as `{nameof(Result.ChildTableSchema)}`,
    rc.table_name as `{nameof(Result.ChildTableName)}`,
    rc.constraint_name as `{nameof(Result.ChildKeyName)}`,
    rc.unique_constraint_name as `{nameof(Result.ParentKeyName)}`,
    case when rc.unique_constraint_name = 'PRIMARY' then 'PRIMARY KEY' else 'UNIQUE' end as `{nameof(Result.ParentKeyType)}`,
    rc.delete_rule as `{nameof(Result.DeleteAction)}`,
    rc.update_rule as `{nameof(Result.UpdateAction)}`
from information_schema.referential_constraints rc
where rc.unique_constraint_schema = @{nameof(Query.SchemaName)} and rc.referenced_table_name = @{nameof(Query.TableName)}
    and rc.unique_constraint_name is not null
order by rc.constraint_schema, rc.table_name, rc.constraint_name
""";
}