namespace SJP.Schematic.MySql.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select view_definition
from information_schema.views
where table_schema = @{ nameof(Query.SchemaName) } and table_name = @{ nameof(Query.ViewName) }
";
}