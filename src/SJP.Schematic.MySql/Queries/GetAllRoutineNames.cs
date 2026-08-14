using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = $"""

select
    routine_schema as `{nameof(Result.SchemaName)}`,
    routine_name as `{nameof(Result.RoutineName)}`
from information_schema.routines
where routine_schema = @{nameof(Query.SchemaName)}
order by routine_schema, routine_name
""";
}