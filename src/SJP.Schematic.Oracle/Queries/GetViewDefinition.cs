namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select TEXT
from SYS.ALL_VIEWS
where OWNER = :{nameof(Query.SchemaName)} and VIEW_NAME = :{nameof(Query.ViewName)}";
}