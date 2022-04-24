namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select v.OWNER as ""{ nameof(Result.SchemaName) }"", v.VIEW_NAME as ""{ nameof(Result.ViewName) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :{ nameof(Query.SchemaName) } and v.VIEW_NAME = :{ nameof(Query.ViewName) } and o.ORACLE_MAINTAINED <> 'Y'";
}