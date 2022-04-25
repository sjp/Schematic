namespace SJP.Schematic.SqlServer.Queries;

internal static class GetRoutineName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }

    internal const string Sql = @$"
select top 1 schema_name(schema_id) as [{ nameof(Result.SchemaName) }], name as [{ nameof(Result.RoutineName) }]
from sys.objects
where schema_id = schema_id(@{ nameof(Query.SchemaName) }) and name = @{ nameof(Query.RoutineName) }
    and type in ('P', 'FN', 'IF', 'TF') and is_ms_shipped = 0";
}