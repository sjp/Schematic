using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableParentKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
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
    }

    internal const string Sql = $"""

select
    c.conname as "{nameof(Result.ChildKeyName)}",
    tc.attname as "{nameof(Result.ColumnName)}",
    child_cols.con_index as "{nameof(Result.ConstraintColumnId)}",
    pns.nspname as "{nameof(Result.ParentSchemaName)}",
    pt.relname as "{nameof(Result.ParentTableName)}",
    coalesce(pkc.contype, 'u') as "{nameof(Result.ParentKeyType)}",
    coalesce(pkc.conname, pki.relname) as "{nameof(Result.ParentKeyName)}",
    c.confupdtype as "{nameof(Result.UpdateAction)}",
    c.confdeltype as "{nameof(Result.DeleteAction)}"
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
-- conparentid = 0 excludes rows pg_constraint adds when the *referenced* table (confrelid) is
-- partitioned: since PG 12, referencing a partitioned table clones one extra constraint row per
-- partition of that table (addFkRecurseReferenced), all sharing this foreign key's name. Those
-- clones' confrelid points at a partition, which is never resolvable as a table on its own.
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid and c.contype = 'f' and c.conparentid = 0
cross join pg_catalog.unnest(c.conkey) with ordinality as child_cols(attnum, con_index)
inner join pg_catalog.pg_attribute tc on tc.attrelid = t.oid and tc.attnum = child_cols.attnum
inner join pg_catalog.pg_class pt on pt.oid = c.confrelid
inner join pg_catalog.pg_namespace pns on pns.oid = pt.relnamespace
-- a foreign key's conindid is the OID of the unique index on the *referenced* table; that index may be
-- backed by a pkey/unique constraint, or it may be a bare unique index with no backing constraint
inner join pg_catalog.pg_class pki on pki.oid = c.conindid
left join pg_catalog.pg_constraint pkc
    on pkc.conindid = c.conindid
    and pkc.conrelid = c.confrelid
    and pkc.contype in ('p', 'u')
where t.relname = @{nameof(Query.TableName)} and ns.nspname = @{nameof(Query.SchemaName)}
""";
}