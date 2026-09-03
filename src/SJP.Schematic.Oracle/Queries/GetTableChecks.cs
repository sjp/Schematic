using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableChecks
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string? Definition { get; init; }

        public required string EnabledStatus { get; init; }

        /// <summary>
        /// <c>VALIDATED</c> when Oracle has verified the existing rows against the constraint,
        /// <c>NOT VALIDATED</c> otherwise.
        /// </summary>
        public required string? ValidatedStatus { get; init; }

        /// <summary>
        /// <c>DEFERRABLE</c> when the constraint's check can be deferred to the end of a transaction,
        /// <c>NOT DEFERRABLE</c> otherwise.
        /// </summary>
        public required string? Deferrable { get; init; }

        /// <summary>
        /// <c>DEFERRED</c> when a deferrable constraint defers by default, <c>IMMEDIATE</c> otherwise.
        /// </summary>
        public required string? Deferred { get; init; }
    }

    internal const string Sql = $"""

select
    CONSTRAINT_NAME as "{nameof(Result.ConstraintName)}",
    SEARCH_CONDITION as "{nameof(Result.Definition)}",
    STATUS as "{nameof(Result.EnabledStatus)}",
    VALIDATED as "{nameof(Result.ValidatedStatus)}",
    DEFERRABLE as "{nameof(Result.Deferrable)}",
    DEFERRED as "{nameof(Result.Deferred)}"
from SYS.ALL_CONSTRAINTS
where OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.TableName)} and CONSTRAINT_TYPE = 'C'
""";
}