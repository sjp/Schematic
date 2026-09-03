using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetUserDefinedTypeChecks
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal sealed record Result : IUserDefinedTypeCheckRow
    {
        public required string? ConstraintName { get; init; }

        public required string? Definition { get; init; }

        /// <summary>
        /// Whether the existing values have been verified against the constraint. <see langword="false" />
        /// for a constraint declared or left <c>NOT VALID</c>.
        /// </summary>
        public required bool IsValidated { get; init; }
    }

    internal const string Sql = $"""

select
    c.conname as "{nameof(Result.ConstraintName)}",
    pg_catalog.pg_get_constraintdef(c.oid) as "{nameof(Result.Definition)}",
    c.convalidated as "{nameof(Result.IsValidated)}"
from pg_catalog.pg_type t
inner join pg_catalog.pg_namespace n on n.oid = t.typnamespace
inner join pg_catalog.pg_constraint c on c.contypid = t.oid and c.contype = 'c'
where n.nspname = @{nameof(Query.SchemaName)} and t.typname = @{nameof(Query.TypeName)}
order by c.conname
""";
}
