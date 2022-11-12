namespace SJP.Schematic.MySql.Queries;

internal static class GetTableComments
{
    internal sealed record Query
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

    internal const string Sql = @$"
-- table
select
    'TABLE' as `{nameof(Result.ObjectType)}`,
    TABLE_NAME as `{nameof(Result.ObjectName)}`,
    TABLE_COMMENT as `{nameof(Result.Comment)}`
from INFORMATION_SCHEMA.TABLES
where TABLE_SCHEMA = @{nameof(Query.SchemaName)} and TABLE_NAME = @{nameof(Query.TableName)}

union

-- columns
select
    'COLUMN' as `{nameof(Result.ObjectType)}`,
    c.COLUMN_NAME as `{nameof(Result.ObjectName)}`,
    c.COLUMN_COMMENT as `{nameof(Result.Comment)}`
from INFORMATION_SCHEMA.COLUMNS c
inner join INFORMATION_SCHEMA.TABLES t on c.TABLE_SCHEMA = t.TABLE_SCHEMA and c.TABLE_NAME = t.TABLE_NAME
where c.TABLE_SCHEMA = @{nameof(Query.SchemaName)} and c.TABLE_NAME = @{nameof(Query.TableName)}

union

-- indexes
select
    'INDEX' as `{nameof(Result.ObjectType)}`,
    s.INDEX_NAME as `{nameof(Result.ObjectName)}`,
    s.INDEX_COMMENT as `{nameof(Result.Comment)}`
from INFORMATION_SCHEMA.STATISTICS s
inner join INFORMATION_SCHEMA.TABLES t on s.TABLE_SCHEMA = t.TABLE_SCHEMA and s.TABLE_NAME = t.TABLE_NAME
where s.TABLE_SCHEMA = @{nameof(Query.SchemaName)} and s.TABLE_NAME = @{nameof(Query.TableName)}
";
}