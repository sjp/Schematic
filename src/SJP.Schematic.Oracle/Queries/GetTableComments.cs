using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ColumnName { get; init; }

        public required string ObjectType { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

-- table
select
    'TABLE' as "{nameof(Result.ObjectType)}",
    NULL as "{nameof(Result.ColumnName)}",
    c.COMMENTS as "{nameof(Result.Comment)}"
from SYS.ALL_TAB_COMMENTS c
where c.OWNER = :{nameof(Query.SchemaName)} and c.TABLE_NAME = :{nameof(Query.TableName)} and c.TABLE_TYPE = 'TABLE'

union all

-- columns
select
    'COLUMN' as "{nameof(Result.ObjectType)}",
    c.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    c.COMMENTS as "{nameof(Result.Comment)}"
from SYS.ALL_COL_COMMENTS c
where c.OWNER = :{nameof(Query.SchemaName)} and c.TABLE_NAME = :{nameof(Query.TableName)}

""";
}