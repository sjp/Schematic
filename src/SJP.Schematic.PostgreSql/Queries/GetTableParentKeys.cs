using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableParentKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result : IForeignKeyColumnRow
    {
        public required string ChildKeyName { get; init; }

        public required string ColumnName { get; init; }

        public required string ParentSchemaName { get; init; }

        public required string ParentTableName { get; init; }

        public required int ConstraintColumnId { get; init; }

        public required string ParentKeyName { get; init; }

        public required string ParentKeyType { get; init; }

        public required string DeleteAction { get; init; }

        public required string UpdateAction { get; init; }

        /// <summary>
        /// Whether the existing rows have been verified against the foreign key. <see langword="false" />
        /// for a foreign key declared or left <c>NOT VALID</c>.
        /// </summary>
        public required bool IsValidated { get; init; }

        /// <summary>
        /// Whether the foreign key was declared <c>DEFERRABLE</c>.
        /// </summary>
        public required bool IsDeferrable { get; init; }

        /// <summary>
        /// Whether a deferrable foreign key was declared <c>INITIALLY DEFERRED</c>.
        /// </summary>
        public required bool IsInitiallyDeferred { get; init; }

        /// <summary>
        /// The <c>MATCH</c> behaviour. <c>s</c> for simple, <c>p</c> for partial, <c>f</c> for full.
        /// </summary>
        public required string MatchType { get; init; }

        /// <summary>
        /// Whether this column is one of the subset listed in <c>ON DELETE SET NULL (...)</c>.
        /// Always <see langword="false" /> before PostgreSQL 15, which had no such subset.
        /// </summary>
        public required bool IsSetNullColumn { get; init; }
    }

    // Each "offset 0" keeps the planner from flattening its lateral subquery into the enclosing join
    // search, so that each per-constraint lookup is planned on its own. As one flat join the query
    // takes many times longer to plan than to run. Removing a fence does not change the result.
    //
    // confdelsetcols (the column subset of ON DELETE SET NULL) only exists from PG 15. Reading it out of
    // the row as jsonb keeps this query parseable on earlier versions, where the key is simply absent and
    // every column reports false. to_jsonb() is cheap here because conbin, the only bulky pg_constraint
    // column, is null for foreign keys.
    //
    // A foreign key's conindid is the OID of the unique index on the *referenced* table; that index may be
    // backed by a pkey/unique constraint, or it may be a bare unique index with no backing constraint.
    //
    // conparentid = 0 excludes rows pg_constraint adds when the *referenced* table (confrelid) is
    // partitioned: since PG 12, referencing a partitioned table clones one extra constraint row per
    // partition of that table (addFkRecurseReferenced), all sharing this foreign key's name. Those
    // clones' confrelid points at a partition, which is never resolvable as a table on its own.
    internal const string Sql = $"""
select
    c.conname as "{nameof(Result.ChildKeyName)}",
    cols.attname as "{nameof(Result.ColumnName)}",
    cols.con_index as "{nameof(Result.ConstraintColumnId)}",
    parent.nspname as "{nameof(Result.ParentSchemaName)}",
    parent.relname as "{nameof(Result.ParentTableName)}",
    parent_key.key_type as "{nameof(Result.ParentKeyType)}",
    parent_key.key_name as "{nameof(Result.ParentKeyName)}",
    c.confupdtype as "{nameof(Result.UpdateAction)}",
    c.confdeltype as "{nameof(Result.DeleteAction)}",
    c.convalidated as "{nameof(Result.IsValidated)}",
    c.condeferrable as "{nameof(Result.IsDeferrable)}",
    c.condeferred as "{nameof(Result.IsInitiallyDeferred)}",
    c.confmatchtype as "{nameof(Result.MatchType)}",
    cols.is_set_null as "{nameof(Result.IsSetNullColumn)}"
from pg_catalog.pg_constraint c
cross join lateral (
    select
        tc.attname,
        child_cols.con_index,
        case
            when pg_catalog.jsonb_typeof(fk_extras.set_null_cols) = 'array'
                then fk_extras.set_null_cols @> pg_catalog.to_jsonb(child_cols.attnum)
            else false
        end as is_set_null
    from (select pg_catalog.to_jsonb(c) -> 'confdelsetcols' as set_null_cols) fk_extras
        cross join pg_catalog.unnest(c.conkey) with ordinality as child_cols(attnum, con_index)
        inner join pg_catalog.pg_attribute tc on tc.attrelid = c.conrelid and tc.attnum = child_cols.attnum
    offset 0
) cols
cross join lateral (
    select pns.nspname, pt.relname
    from pg_catalog.pg_class pt
        inner join pg_catalog.pg_namespace pns on pns.oid = pt.relnamespace
    where pt.oid = c.confrelid
    offset 0
) parent
cross join lateral (
    select coalesce(pkc.contype, 'u') as key_type, coalesce(pkc.conname, pki.relname) as key_name
    from pg_catalog.pg_class pki
        left join pg_catalog.pg_constraint pkc
            on pkc.conindid = pki.oid
            and pkc.conrelid = c.confrelid
            and pkc.contype in ('p', 'u')
    where pki.oid = c.conindid
    offset 0
) parent_key
where c.conrelid = (
    select t.oid
    from pg_catalog.pg_class t
        inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    where t.relname = @{nameof(Query.TableName)} and ns.nspname = @{nameof(Query.SchemaName)}
)
    and c.contype = 'f' and c.conparentid = 0
""";
}