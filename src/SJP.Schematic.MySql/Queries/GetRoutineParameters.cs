using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetRoutineParameters
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// The parameter's position in the signature. Zero is not a parameter but a function's
        /// return value, which MySQL stores in the same table.
        /// </summary>
        public required int Ordinal { get; init; }

        public required string? ParameterName { get; init; }

        /// <summary>
        /// <c>IN</c>, <c>OUT</c> or <c>INOUT</c>, and <see langword="null" /> for a return value.
        /// </summary>
        public required string? ParameterMode { get; init; }

        public required string DataTypeName { get; init; }

        public required int CharacterMaxLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required string? Collation { get; init; }
    }

    internal const string Sql = $"""

select
    ordinal_position as `{nameof(Result.Ordinal)}`,
    parameter_name as `{nameof(Result.ParameterName)}`,
    parameter_mode as `{nameof(Result.ParameterMode)}`,
    data_type as `{nameof(Result.DataTypeName)}`,
    coalesce(character_maximum_length, 0) as `{nameof(Result.CharacterMaxLength)}`,
    coalesce(numeric_precision, 0) as `{nameof(Result.Precision)}`,
    coalesce(numeric_scale, 0) as `{nameof(Result.Scale)}`,
    collation_name as `{nameof(Result.Collation)}`
from information_schema.parameters
where specific_schema = @{nameof(Query.SchemaName)} and specific_name = @{nameof(Query.RoutineName)}
order by ordinal_position
""";
}
