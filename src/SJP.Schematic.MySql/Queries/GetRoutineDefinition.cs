using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        public required string Definition { get; init; }

        /// <summary>
        /// <c>PROCEDURE</c> or <c>FUNCTION</c>.
        /// </summary>
        public required string RoutineType { get; init; }

        /// <summary>
        /// The language the body is written in. Always <c>SQL</c> in MySQL, which supports no other.
        /// </summary>
        public required string? Language { get; init; }
    }

    // MySQL keys routines on (schema, name, type), so a schema may hold a procedure and a function
    // that share a name. Only one routine is exposed per name, as elsewhere in this provider.
    internal const string Sql = $"""

select
    routine_definition as `{nameof(Result.Definition)}`,
    routine_type as `{nameof(Result.RoutineType)}`,
    routine_body as `{nameof(Result.Language)}`
from information_schema.routines
where routine_schema = @{nameof(Query.SchemaName)} and routine_name = @{nameof(Query.RoutineName)}
limit 1
""";
}
