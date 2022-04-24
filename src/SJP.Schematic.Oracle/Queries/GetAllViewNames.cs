namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllViewNames
{
    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    v.OWNER as ""{ nameof(Result.SchemaName) }"",
    v.VIEW_NAME as ""{ nameof(Result.ViewName) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by v.OWNER, v.VIEW_NAME";
}