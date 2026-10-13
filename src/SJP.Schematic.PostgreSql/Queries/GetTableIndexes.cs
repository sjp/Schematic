using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? IndexName { get; init; }

        public required bool IsUnique { get; init; }

        public required bool IsPrimary { get; init; }

        public required string? FilterDefinition { get; init; }

        public required int KeyColumnCount { get; init; }

        public required int IndexColumnId { get; init; }

        public required string? IndexColumnExpression { get; init; }

        /// <summary>
        /// The unquoted name of the indexed column, or <c>null</c> when the index column is an expression.
        /// </summary>
        public required string? IndexColumnName { get; init; }

        public required bool IsDescending { get; init; }

        public required bool IsNullsFirst { get; init; }

        public required string? IndexColumnCollation { get; init; }

        /// <summary>
        /// The name of the access method implementing the index, e.g. <c>btree</c> or <c>gin</c>.
        /// </summary>
        public required string IndexMethod { get; init; }

        /// <summary>
        /// Whether the index was built completely and can therefore be used by the query planner.
        /// </summary>
        public required bool IsValid { get; init; }

        /// <summary>
        /// The name of the primary key or unique constraint the index enforces, or <c>null</c> when it enforces neither.
        /// </summary>
        public string? ConstraintName { get; init; }

        /// <summary>
        /// Whether the constraint the index enforces was declared <c>DEFERRABLE</c>.
        /// </summary>
        public bool IsDeferrable { get; init; }

        /// <summary>
        /// Whether the deferrable constraint the index enforces was declared <c>INITIALLY DEFERRED</c>.
        /// </summary>
        public bool IsInitiallyDeferred { get; init; }
    }

    // Index column name: the raw column name, unlike pg_get_indexdef which quotes it where required;
    // expression columns are stored as attribute number 0, which matches no attribute and so gives null.
    //
    // Descending: the property is null for included columns and for access methods without an ordering.
    //
    // Collation: the default collation is not worth reporting, it is simply the one the column already has.
    //
    // Constraint: every primary key and unique constraint is enforced by an index whose key columns are the
    // constraint's columns in order, so the keys are read from the same rows as the indexes. The contype filter
    // matters: a foreign key on the same table that references this index also points at it through conindid.
    internal const string Sql = $"""
select
    i.relname as "{nameof(Result.IndexName)}",
    idx.indisunique as "{nameof(Result.IsUnique)}",
    idx.indisprimary as "{nameof(Result.IsPrimary)}",
    pg_catalog.pg_get_expr(idx.indpred, idx.indrelid) as "{nameof(Result.FilterDefinition)}",
    idx.indnkeyatts as "{nameof(Result.KeyColumnCount)}",
    k.k as "{nameof(Result.IndexColumnId)}",
    pg_catalog.pg_get_indexdef(idx.indexrelid, k.k + 1, true) as "{nameof(Result.IndexColumnExpression)}",
    a.attname::text as "{nameof(Result.IndexColumnName)}",
    coalesce(pg_catalog.pg_index_column_has_property(idx.indexrelid, k.k + 1, 'desc'), false) as "{nameof(Result.IsDescending)}",
    coalesce(pg_catalog.pg_index_column_has_property(idx.indexrelid, k.k + 1, 'nulls_first'), false) as "{nameof(Result.IsNullsFirst)}",
    nullif(coll.collname, 'default') as "{nameof(Result.IndexColumnCollation)}",
    am.amname as "{nameof(Result.IndexMethod)}",
    idx.indisvalid as "{nameof(Result.IsValid)}",
    kc.conname as "{nameof(Result.ConstraintName)}",
    coalesce(kc.condeferrable, false) as "{nameof(Result.IsDeferrable)}",
    coalesce(kc.condeferred, false) as "{nameof(Result.IsInitiallyDeferred)}"
from pg_catalog.pg_index idx
    inner join pg_catalog.pg_class i on i.oid = idx.indexrelid
    inner join pg_catalog.pg_am am on am.oid = i.relam
    cross join lateral pg_catalog.generate_subscripts(idx.indkey, 1) as k(k)
    left join pg_catalog.pg_attribute a on a.attrelid = idx.indrelid and a.attnum = idx.indkey[k.k]
    left join pg_catalog.pg_collation coll on coll.oid = idx.indcollation[k.k]
    left join pg_catalog.pg_constraint kc
        on kc.conrelid = idx.indrelid
        and kc.conindid = idx.indexrelid
        and kc.contype in ('p', 'u')
where idx.indrelid = (
    select t.oid
    from pg_catalog.pg_class t
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    where t.relkind in ('r', 'p')
        and t.relname = @{nameof(Query.TableName)}
        and ns.nspname = @{nameof(Query.SchemaName)}
)
order by i.relname, "{nameof(Result.IndexColumnId)}"
""";
}