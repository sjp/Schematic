using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = $"""

select routine_definition
from information_schema.routines
where routine_schema = @{nameof(Query.SchemaName)} and routine_name = @{nameof(Query.RoutineName)}
limit 1
""";
}