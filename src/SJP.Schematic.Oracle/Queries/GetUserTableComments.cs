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

    internal const string Sql = @$"
-- table
select
    'TABLE' as ""{nameof(Result.ObjectType)}"",
    NULL as ""{nameof(Result.ColumnName)}"",
    c.COMMENTS as ""{nameof(Result.Comment)}""
from SYS.USER_TABLES t
left join SYS.USER_MVIEWS mv on t.TABLE_NAME = mv.MVIEW_NAME
left join SYS.USER_TAB_COMMENTS c on t.TABLE_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'TABLE'
where t.TABLE_NAME = :{nameof(Query.TableName)} and mv.MVIEW_NAME is null

union

-- columns
select
    'COLUMN' as ""{nameof(Result.ObjectType)}"",
    tc.COLUMN_NAME as ""{nameof(Result.ColumnName)}"",
    c.COMMENTS as ""{nameof(Result.Comment)}""
from SYS.USER_TABLES t
left join SYS.USER_MVIEWS mv on t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.USER_TAB_COLS tc on tc.TABLE_NAME = t.TABLE_NAME
left join SYS.USER_COL_COMMENTS c on c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
where t.TABLE_NAME = :{nameof(Query.TableName)} and mv.MVIEW_NAME is null
";
}