using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ObjectType { get; init; }

        public required string ObjectName { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

-- table
select
    'TABLE' as `{nameof(Result.ObjectType)}`,
    table_name as `{nameof(Result.ObjectName)}`,
    table_comment as `{nameof(Result.Comment)}`
from information_schema.tables
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.TableName)}

union all

-- columns
select
    'COLUMN' as `{nameof(Result.ObjectType)}`,
    column_name as `{nameof(Result.ObjectName)}`,
    column_comment as `{nameof(Result.Comment)}`
from information_schema.columns
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.TableName)}

union all

-- indexes
select distinct
    'INDEX' as `{nameof(Result.ObjectType)}`,
    index_name as `{nameof(Result.ObjectName)}`,
    index_comment as `{nameof(Result.Comment)}`
from information_schema.statistics
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.TableName)}
""";
}