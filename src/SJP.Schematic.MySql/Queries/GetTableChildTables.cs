using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableChildTables
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
    }

    // Only the tables holding the foreign keys are read here. Each foreign key is then built from the child
    // table's own cached foreign key rows, which are read by key and describe the constraint in full.
    //
    // Child tables can be in any schema, so referential_constraints is filtered on the referenced table only.
    // MariaDB cannot look that up by key and still reads every table on the server to answer it.
    internal const string Sql = $"""

select distinct
    rc.constraint_schema as `{nameof(Result.ChildTableSchema)}`,
    rc.table_name as `{nameof(Result.ChildTableName)}`
from information_schema.referential_constraints rc
where rc.unique_constraint_schema = @{nameof(Query.SchemaName)} and rc.referenced_table_name = @{nameof(Query.TableName)}
    and rc.unique_constraint_name is not null
order by rc.constraint_schema, rc.table_name
""";
}
