using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetRoutineName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select top 1 schema_name(schema_id) as [{nameof(Result.SchemaName)}], name as [{nameof(Result.RoutineName)}]
from sys.objects
where schema_id = schema_id(@{nameof(Query.SchemaName)}) and name = @{nameof(Query.RoutineName)}
    and type in ('P', 'FN', 'IF', 'TF') and is_ms_shipped = 0";
}