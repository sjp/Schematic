namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllViewNames
{
    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal const string Sql = @$"
select schema_name(schema_id) as [{ nameof(Result.SchemaName) }], name as [{ nameof(Result.ViewName) }]
from sys.views
where is_ms_shipped = 0
order by schema_name(schema_id), name";
}