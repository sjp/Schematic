namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select TEXT
from SYS.ALL_VIEWS
where OWNER = :{nameof(Query.SchemaName)} and VIEW_NAME = :{nameof(Query.ViewName)}";
}