using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    /// <summary>
    /// Either a routine's definition row or one of its parameter rows. A definition row has no <see cref="Ordinal"/>.
    /// </summary>
    internal sealed record Result
    {
        public required string RoutineName { get; init; }

        /// <summary>
        /// <c>PROCEDURE</c> or <c>FUNCTION</c>.
        /// </summary>
        public required string RoutineType { get; init; }

        /// <summary>
        /// The routine body. <see langword="null" /> on parameter rows, and on the definition row unless the user
        /// defined the routine or may read routine bodies.
        /// </summary>
        public required string? Definition { get; init; }

        /// <summary>
        /// The language the body is written in. Always <c>SQL</c> in MySQL, which supports no other.
        /// </summary>
        public required string? Language { get; init; }

        /// <summary>
        /// The parameter's position in the signature, and <see langword="null" /> on the definition row. Zero is not
        /// a parameter but a function's return value, which MySQL stores in the same table.
        /// </summary>
        public required int? Ordinal { get; init; }

        public required string? ParameterName { get; init; }

        /// <summary>
        /// <c>IN</c>, <c>OUT</c> or <c>INOUT</c>, and <see langword="null" /> for a return value.
        /// </summary>
        public required string? ParameterMode { get; init; }

        public required string? DataTypeName { get; init; }

        /// <summary>
        /// The parameter's maximum length in characters.
        /// </summary>
        public required long? CharacterMaxLength { get; init; }

        public required int? Precision { get; init; }

        public required int? Scale { get; init; }

        public required string? Collation { get; init; }
    }

    // MySQL keys routines on (schema, name, type), so a schema may hold a procedure and a function
    // that share a name. Both are returned here, and the caller keeps one of them.
    // The definition row is told apart from the parameter rows by its null ordinal.
    // No "order by": on MariaDB any ordering over information_schema.routines or parameters disables
    // the schema/name lookup, so every routine on the server is read and parsed. Callers sort by ordinal.
    internal const string Sql = $"""

select
    routine_name as `{nameof(Result.RoutineName)}`,
    routine_type as `{nameof(Result.RoutineType)}`,
    routine_definition as `{nameof(Result.Definition)}`,
    routine_body as `{nameof(Result.Language)}`,
    null as `{nameof(Result.Ordinal)}`,
    null as `{nameof(Result.ParameterName)}`,
    null as `{nameof(Result.ParameterMode)}`,
    null as `{nameof(Result.DataTypeName)}`,
    null as `{nameof(Result.CharacterMaxLength)}`,
    null as `{nameof(Result.Precision)}`,
    null as `{nameof(Result.Scale)}`,
    null as `{nameof(Result.Collation)}`
from information_schema.routines
where routine_schema = @{nameof(Query.SchemaName)} and routine_name = @{nameof(Query.RoutineName)}
union all
select
    specific_name,
    routine_type,
    null,
    null,
    ordinal_position,
    parameter_name,
    parameter_mode,
    data_type,
    coalesce(character_maximum_length, 0),
    coalesce(numeric_precision, 0),
    coalesce(numeric_scale, 0),
    collation_name
from information_schema.parameters
where specific_schema = @{nameof(Query.SchemaName)} and specific_name = @{nameof(Query.RoutineName)}
""";
}
