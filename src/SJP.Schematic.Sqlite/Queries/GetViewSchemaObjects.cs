using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetViewSchemaObjects
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        public required string Type { get; init; }

        public required string Name { get; init; }

        public string? Sql { get; init; }
    }

    internal const string ViewType = "view";

    internal const string TriggerType = "trigger";

    // sqlite_master has no index, so reading the view and its triggers together costs one scan
    // instead of two. A trigger's tbl_name keeps the spelling of its ON clause, which SQLite matches
    // ignoring the case of ASCII letters, as nocase does. View names are unique ignoring that case
    // too, so at most one view row is returned.
    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return $"""

select
    type as "{nameof(Result.Type)}",
    name as "{nameof(Result.Name)}",
    sql as "{nameof(Result.Sql)}"
from {dialect.QuoteIdentifier(schemaName)}.sqlite_master
where tbl_name = @{nameof(Query.ViewName)} collate nocase and type in ('{ViewType}', '{TriggerType}')
""";
    }
}
