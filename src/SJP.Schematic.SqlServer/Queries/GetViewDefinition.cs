namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select sm.definition
from sys.sql_modules sm
inner join sys.views v on sm.object_id = v.object_id
where schema_name(v.schema_id) = @{nameof(Query.SchemaName)} and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0";
}