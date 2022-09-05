using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Queries;

internal static class GetTriggerDefinition
{
    internal sealed record Query
    {
        public string TableName { get; set; } = default!;
    }

    internal sealed record Result
    {
        public string Type { get; init; } = default!;

        public string Name { get; init; } = default!;

        public string TableName { get; init; } = default!;

        public long RootPage { get; init; }

        public string Sql { get; init; } = default!;
    }

    internal static string Sql(IDatabaseDialect dialect, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(dialect);
        if (schemaName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schemaName));

        return $@"
select
    type AS ""{nameof(Result.Type)}"",
    name AS ""{nameof(Result.Name)}"",
    tbl_name AS ""{nameof(Result.TableName)}"",
    rootpage AS ""{nameof(Result.RootPage)}"",
    sql AS ""{nameof(Result.Sql)}""
from {dialect.QuoteIdentifier(schemaName)}.sqlite_master
where type = 'trigger' and tbl_name = @{nameof(Query.TableName)}";
    }
}