using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetRoutineSignature
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// <c>FUNCTION</c> or <c>PROCEDURE</c>.
        /// </summary>
        public required string RoutineType { get; init; }

        /// <summary>
        /// The argument's position in the signature. Zero is a function's return value, and the
        /// whole row is null for a routine that takes no arguments.
        /// </summary>
        public required int? Position { get; init; }

        public required string? ArgumentName { get; init; }

        /// <summary>
        /// <c>IN</c>, <c>OUT</c> or <c>IN/OUT</c>.
        /// </summary>
        public required string? InOut { get; init; }

        public required string? ArgumentTypeSchema { get; init; }

        public required string? ArgumentTypeName { get; init; }

        public required int DataLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        /// <summary>
        /// <c>Y</c> when the argument has a default. Oracle exposes the default's text as a
        /// <c>LONG</c>, which cannot be read alongside the rest of the row, so only its presence
        /// is available here.
        /// </summary>
        public required string? Defaulted { get; init; }
    }

    // ALL_ARGUMENTS also describes package subprograms and the fields of composite arguments, hence
    // the PACKAGE_NAME and DATA_LEVEL predicates. The outer join keeps the routine's type available
    // for a routine that declares no arguments at all.
    internal const string Sql = $"""

select
    o.OBJECT_TYPE as "{nameof(Result.RoutineType)}",
    a.POSITION as "{nameof(Result.Position)}",
    a.ARGUMENT_NAME as "{nameof(Result.ArgumentName)}",
    a.IN_OUT as "{nameof(Result.InOut)}",
    a.TYPE_OWNER as "{nameof(Result.ArgumentTypeSchema)}",
    a.DATA_TYPE as "{nameof(Result.ArgumentTypeName)}",
    nvl(a.DATA_LENGTH, 0) as "{nameof(Result.DataLength)}",
    nvl(a.DATA_PRECISION, 0) as "{nameof(Result.Precision)}",
    nvl(a.DATA_SCALE, 0) as "{nameof(Result.Scale)}",
    a.DEFAULTED as "{nameof(Result.Defaulted)}"
from SYS.ALL_OBJECTS o
left join SYS.ALL_ARGUMENTS a
    on a.OWNER = o.OWNER and a.OBJECT_NAME = o.OBJECT_NAME
    and a.PACKAGE_NAME is null and a.DATA_LEVEL = 0
where o.OWNER = :{nameof(Query.SchemaName)} and o.OBJECT_NAME = :{nameof(Query.RoutineName)}
    and o.ORACLE_MAINTAINED <> 'Y' and o.OBJECT_TYPE in ('FUNCTION', 'PROCEDURE')
order by a.POSITION
""";
}
