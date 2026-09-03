using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTablePrimaryKey
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string ColumnName { get; init; }

        public required int OrdinalPosition { get; init; }

        /// <summary>
        /// Whether the constraint was declared <c>DEFERRABLE</c>.
        /// </summary>
        public required bool IsDeferrable { get; init; }

        /// <summary>
        /// Whether a deferrable constraint was declared <c>INITIALLY DEFERRED</c>.
        /// </summary>
        public required bool IsInitiallyDeferred { get; init; }
    }

    internal const string Sql = $"""

select
    c.conname as "{nameof(Result.ConstraintName)}",
    a.attname as "{nameof(Result.ColumnName)}",
    con_cols.ordinal_position as "{nameof(Result.OrdinalPosition)}",
    c.condeferrable as "{nameof(Result.IsDeferrable)}",
    c.condeferred as "{nameof(Result.IsInitiallyDeferred)}"
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid and c.contype = 'p'
cross join pg_catalog.unnest(c.conkey) with ordinality as con_cols(attnum, ordinal_position)
inner join pg_catalog.pg_attribute a on a.attrelid = t.oid and a.attnum = con_cols.attnum
where t.relkind in ('r', 'p')
    and t.relname = @{nameof(Query.TableName)}
    and ns.nspname = @{nameof(Query.SchemaName)}
""";
}