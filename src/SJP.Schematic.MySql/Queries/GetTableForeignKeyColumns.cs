using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableForeignKeyColumns
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result : IForeignKeyColumnRow
    {
        public required string ChildKeyName { get; init; }

        public required string ColumnName { get; init; }
    }

    // table_schema and table_name are the columns MariaDB can look key_column_usage up by, so this only
    // opens the one table instead of scanning every table on the server.
    internal const string Sql = $"""

select
    kc.constraint_name as `{nameof(Result.ChildKeyName)}`,
    kc.column_name as `{nameof(Result.ColumnName)}`
from information_schema.key_column_usage kc
where kc.table_schema = @{nameof(Query.SchemaName)} and kc.table_name = @{nameof(Query.TableName)}
    and kc.referenced_table_name is not null
order by kc.constraint_name, kc.ordinal_position
""";
}
