namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllMaterializedViewNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select
    mv.OWNER as ""{nameof(Result.SchemaName)}"",
    mv.MVIEW_NAME as ""{nameof(Result.ViewName)}""
from SYS.ALL_MVIEWS mv
inner join SYS.ALL_OBJECTS o on mv.OWNER = o.OWNER and mv.MVIEW_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y' and o.OBJECT_TYPE <> 'TABLE'
order by mv.OWNER, mv.MVIEW_NAME";
}