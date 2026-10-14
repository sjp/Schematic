using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTableSchemaObjects
{
    internal sealed record Query : ISqlQuery<SqliteTableSchemaObject>
    {
        public required string TableName { get; init; }
    }

    internal const string TableType = "table";

    internal const string IndexType = "index";

    internal const string TriggerType = "trigger";

    // sqlite_master has no index, so reading the table, its indexes and its triggers together costs
    // one scan instead of three. A trigger's tbl_name keeps the spelling of its ON clause, which
    // SQLite matches ignoring the case of ASCII letters, as nocase does.
    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"""

select
    type as "{nameof(SqliteTableSchemaObject.Type)}",
    name as "{nameof(SqliteTableSchemaObject.Name)}",
    sql as "{nameof(SqliteTableSchemaObject.Sql)}"
from {dialect.QuoteIdentifier(schemaName)}.sqlite_master
where tbl_name = @{nameof(Query.TableName)} collate nocase and type in ('{TableType}', '{IndexType}', '{TriggerType}')
""";
    }
}
