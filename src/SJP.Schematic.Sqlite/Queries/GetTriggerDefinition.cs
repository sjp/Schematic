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
        public required string Type { get; init; }

        public required string Name { get; init; }

        public required string TableName { get; init; }

        public required long RootPage { get; init; }

        public required string Sql { get; init; }
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