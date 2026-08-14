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

    internal const string Sql = $"""

select
    rc.constraint_schema as `{nameof(Result.ChildTableSchema)}`,
    rc.table_name as `{nameof(Result.ChildTableName)}`,
    rc.constraint_name as `{nameof(Result.ChildKeyName)}`,
    rc.unique_constraint_name as `{nameof(Result.ParentKeyName)}`,
    ptc.constraint_type as `{nameof(Result.ParentKeyType)}`,
    rc.delete_rule as `{nameof(Result.DeleteAction)}`,
    rc.update_rule as `{nameof(Result.UpdateAction)}`
from information_schema.referential_constraints rc
inner join information_schema.table_constraints ptc
    on ptc.table_schema = rc.unique_constraint_schema
    and ptc.table_name = rc.referenced_table_name
    and ptc.constraint_name = rc.unique_constraint_name
where rc.unique_constraint_schema = @{nameof(Query.SchemaName)} and rc.referenced_table_name = @{nameof(Query.TableName)}
order by rc.constraint_schema, rc.table_name, rc.constraint_name
""";
}