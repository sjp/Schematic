using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserTableComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
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
from SYS.USER_TAB_COMMENTS c
where c.TABLE_NAME = :{nameof(Query.TableName)} and c.TABLE_TYPE = 'TABLE'

union all

-- columns
select
    'COLUMN' as "{nameof(Result.ObjectType)}",
    c.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    c.COMMENTS as "{nameof(Result.Comment)}"
from SYS.USER_COL_COMMENTS c
where c.TABLE_NAME = :{nameof(Query.TableName)}

""";
}