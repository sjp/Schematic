using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableChecks
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? ConstraintName { get; init; }

        public required string? Definition { get; init; }

        /// <summary>
        /// Whether the existing rows have been verified against the constraint. <see langword="false" />
        /// for a constraint declared or left <c>NOT VALID</c>.
        /// </summary>
        public required bool IsValidated { get; init; }
    }

    internal const string Sql = $"""

select
    c.conname as "{nameof(Result.ConstraintName)}",
    pg_catalog.pg_get_constraintdef(c.oid) as "{nameof(Result.Definition)}",
    c.convalidated as "{nameof(Result.IsValidated)}"
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @{nameof(Query.TableName)}
    and ns.nspname = @{nameof(Query.SchemaName)}
    -- defensive guard against a partition-cloned check constraint, matching GetTableChildKeys and
    -- GetTableParentKeys. Provably a no-op today: c.conrelid = t.oid alone already makes
    -- (conrelid, contypid, conname) unique, so duplicates cannot occur for any table this provider
    -- resolves. Deliberately NOT conislocal/coninhcount -- those would incorrectly drop checks
    -- inherited via legacy CREATE TABLE ... INHERITS, whose children are ordinary tables that this
    -- provider does expose and that do enforce the inherited check.
    and c.conparentid = 0
""";
}