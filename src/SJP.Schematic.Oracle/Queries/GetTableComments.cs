namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableComments
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ColumnName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string? Comment { get; init; }
    }

    internal const string Sql = @$"
-- table
select
    'TABLE' as ""{nameof(Result.ObjectType)}"",
    NULL as ""{nameof(Result.ColumnName)}"",
    c.COMMENTS as ""{nameof(Result.Comment)}""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on t.OWNER = c.OWNER and t.TABLE_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'TABLE'
where t.OWNER = :{nameof(Query.SchemaName)} and t.TABLE_NAME = :{nameof(Query.TableName)}
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null

union

-- columns
select
    'COLUMN' as ""{nameof(Result.ObjectType)}"",
    tc.COLUMN_NAME as ""{nameof(Result.ColumnName)}"",
    c.COMMENTS as ""{nameof(Result.Comment)}""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS tc on tc.OWNER = t.OWNER and tc.TABLE_NAME = t.TABLE_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = tc.OWNER and c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
where t.OWNER = :{nameof(Query.SchemaName)} and t.TABLE_NAME = :{nameof(Query.TableName)}
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null
";
}