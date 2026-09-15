using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableCheckConstraints
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string Definition { get; init; }

        public required string Enforced { get; init; }
    }

    internal const string Sql = $"""

select
    cc.constraint_name as `{nameof(Result.ConstraintName)}`,
    cc.check_clause as `{nameof(Result.Definition)}`,
    tc.enforced as `{nameof(Result.Enforced)}`
from information_schema.table_constraints tc
inner join information_schema.check_constraints cc on tc.table_schema = cc.constraint_schema and tc.constraint_name = cc.constraint_name
where tc.table_schema = @{nameof(Query.SchemaName)} and tc.table_name = @{nameof(Query.TableName)} and tc.constraint_type = 'CHECK'
""";

    // For a server whose table_constraints has no enforced column, and so no NOT ENFORCED syntax, meaning
    // a check constraint is always enforced.
    internal const string SqlWithoutEnforced = $"""

select
    cc.constraint_name as `{nameof(Result.ConstraintName)}`,
    cc.check_clause as `{nameof(Result.Definition)}`,
    'YES' as `{nameof(Result.Enforced)}`
from information_schema.table_constraints tc
inner join information_schema.check_constraints cc on tc.table_schema = cc.constraint_schema and tc.constraint_name = cc.constraint_name
where tc.table_schema = @{nameof(Query.SchemaName)} and tc.table_name = @{nameof(Query.TableName)} and tc.constraint_type = 'CHECK'
""";

    // For a server whose check_constraints has a table_name column (MariaDB). MariaDB names check constraints
    // per table, so two tables in one schema can both have one called CONSTRAINT_1, and matching on the
    // constraint name alone would return the other table's check too. MariaDB can also only avoid opening
    // every table on the server when both the schema and the table name are constants. MariaDB has no
    // NOT ENFORCED syntax, so a check constraint is always enforced.
    internal const string SqlByTableName = $"""

select
    cc.constraint_name as `{nameof(Result.ConstraintName)}`,
    cc.check_clause as `{nameof(Result.Definition)}`,
    'YES' as `{nameof(Result.Enforced)}`
from information_schema.check_constraints cc
where cc.constraint_schema = @{nameof(Query.SchemaName)} and cc.table_name = @{nameof(Query.TableName)}
""";
}
