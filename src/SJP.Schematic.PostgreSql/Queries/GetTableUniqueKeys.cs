using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

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

        public required int OrdinalPosition { get; init; }
    }

    internal const string Sql = $"""

select
    kc.constraint_name as "{nameof(Result.ConstraintName)}",
    kc.column_name as "{nameof(Result.ColumnName)}",
    kc.ordinal_position as "{nameof(Result.OrdinalPosition)}"
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @{nameof(Query.SchemaName)} and tc.table_name = @{nameof(Query.TableName)}
    and tc.constraint_type = 'UNIQUE'
""";
}