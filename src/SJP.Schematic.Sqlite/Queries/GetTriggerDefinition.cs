using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTriggerDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string Name { get; init; }

        public required string Sql { get; init; }
    }

    // a trigger's tbl_name keeps the spelling of its ON clause, which SQLite matches ignoring the
    // case of ASCII letters, as nocase does
    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"""

select
    name as "{nameof(Result.Name)}",
    sql as "{nameof(Result.Sql)}"
from {dialect.QuoteIdentifier(schemaName)}.sqlite_master
where type = 'trigger' and tbl_name = @{nameof(Query.TableName)} collate nocase
""";
    }
}