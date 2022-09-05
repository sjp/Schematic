namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserViewComments
{
    internal sealed record Query
    {
        public string ViewName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ColumnName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string? Comment { get; init; }
    }

    internal const string Sql = @$"
-- view
select
    'VIEW' as ""{nameof(Result.ObjectType)}"",
    NULL as ""{nameof(Result.ColumnName)}"",
    c.COMMENTS as ""{nameof(Result.Comment)}""
from SYS.USER_VIEWS v
left join SYS.USER_TAB_COMMENTS c on v.VIEW_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'VIEW'
where v.VIEW_NAME = :{nameof(Query.ViewName)}
union

-- columns
select
    'COLUMN' as ""{nameof(Result.ObjectType)}"",
    vc.COLUMN_NAME as ""{nameof(Result.ColumnName)}"",
    c.COMMENTS as ""{nameof(Result.Comment)}""
from SYS.USER_VIEWS v
inner join SYS.USER_TAB_COLS vc on vc.TABLE_NAME = v.VIEW_NAME
left join SYS.USER_COL_COMMENTS c on c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.VIEW_NAME = :{nameof(Query.ViewName)}
";
}