using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal const string Sql = @$"
select table_schema as `{nameof(Result.SchemaName)}`, table_name as `{nameof(Result.TableName)}`
from information_schema.tables
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.TableName)}
limit 1";
}