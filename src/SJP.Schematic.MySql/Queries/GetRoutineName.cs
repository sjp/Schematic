using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

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

        /// <summary>
        /// <c>PROCEDURE</c> or <c>FUNCTION</c>.
        /// </summary>
        public required string RoutineType { get; init; }
    }

    // Returns a row per routine type, as a function and a procedure may share a name.
    internal const string Sql = $"""

select
    routine_schema as `{nameof(Result.SchemaName)}`,
    routine_name as `{nameof(Result.RoutineName)}`,
    routine_type as `{nameof(Result.RoutineType)}`
from information_schema.routines
where routine_schema = @{nameof(Query.SchemaName)} and routine_name = @{nameof(Query.RoutineName)}
""";
}