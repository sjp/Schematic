namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewName
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select top 1 schema_name(schema_id) as [{nameof(Result.SchemaName)}], name as [{nameof(Result.ViewName)}]
from sys.views
where schema_id = schema_id(@{nameof(Query.SchemaName)}) and name = @{nameof(Query.ViewName)}
    and is_ms_shipped = 0";
}