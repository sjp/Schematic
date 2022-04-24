namespace SJP.Schematic.Oracle.Queries;

internal static class GetMaterializedViewDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select QUERY
from SYS.ALL_MVIEWS
where OWNER = :{ nameof(Query.SchemaName) } and MVIEW_NAME = :{ nameof(Query.ViewName) }";
}