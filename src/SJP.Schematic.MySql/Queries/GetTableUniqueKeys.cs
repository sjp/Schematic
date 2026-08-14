using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableUniqueKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string ColumnName { get; init; }
    }

    internal const string Sql = $"""

select
    kc.constraint_name as `{nameof(Result.ConstraintName)}`,
    kc.column_name as `{nameof(Result.ColumnName)}`
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on kc.constraint_schema = tc.constraint_schema
    and kc.constraint_name = tc.constraint_name
    and kc.table_schema = tc.table_schema
    and kc.table_name = tc.table_name
where tc.table_schema = @{nameof(Query.SchemaName)} and tc.table_name = @{nameof(Query.TableName)}
    and tc.constraint_type = 'UNIQUE'
order by kc.ordinal_position
""";
}