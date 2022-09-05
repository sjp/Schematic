namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal const string Sql = @$"
select t.OWNER as ""{nameof(Result.SchemaName)}"", t.TABLE_NAME as ""{nameof(Result.TableName)}""
from SYS.ALL_TABLES t
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
left join SYS.ALL_NESTED_TABLES nt on t.OWNER = nt.OWNER and t.TABLE_NAME = nt.TABLE_NAME
left join SYS.ALL_EXTERNAL_TABLES et on t.OWNER = et.OWNER and t.TABLE_NAME = et.TABLE_NAME
where
    t.OWNER = :{nameof(Query.SchemaName)} and t.TABLE_NAME = :{nameof(Query.TableName)}
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and o.SUBOBJECT_NAME is null
    and mv.MVIEW_NAME is null
    and nt.TABLE_NAME is null
    and et.TABLE_NAME is null";
}