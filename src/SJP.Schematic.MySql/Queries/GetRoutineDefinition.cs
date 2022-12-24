using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select
    ROUTINE_DEFINITION
from information_schema.routines
where
    ROUTINE_SCHEMA = @{nameof(Query.SchemaName)}
    and ROUTINE_NAME = @{nameof(Query.RoutineName)}";
}