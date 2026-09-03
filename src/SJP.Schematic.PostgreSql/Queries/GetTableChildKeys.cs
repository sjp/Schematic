using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableChildKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ChildTableSchema { get; init; }

        public required string ChildTableName { get; init; }

        public required string ChildKeyName { get; init; }

        public required string ParentKeyName { get; init; }

        public required string ParentKeyType { get; init; }

        public required string DeleteAction { get; init; }

        public required string UpdateAction { get; init; }
    }

    internal const string Sql = $"""

select
    ns.nspname as "{nameof(Result.ChildTableSchema)}",
    t.relname as "{nameof(Result.ChildTableName)}",
    c.conname as "{nameof(Result.ChildKeyName)}",
    coalesce(pkc.contype, 'u') as "{nameof(Result.ParentKeyType)}",
    coalesce(pkc.conname, pki.relname) as "{nameof(Result.ParentKeyName)}",
    c.confupdtype as "{nameof(Result.UpdateAction)}",
    c.confdeltype as "{nameof(Result.DeleteAction)}"
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid and c.contype = 'f'
inner join pg_catalog.pg_class pt on pt.oid = c.confrelid
inner join pg_catalog.pg_namespace pns on pns.oid = pt.relnamespace
-- a foreign key's conindid is the OID of the unique index on the *referenced* table; that index may be
-- backed by a pkey/unique constraint, or it may be a bare unique index with no backing constraint
inner join pg_catalog.pg_class pki on pki.oid = c.conindid
left join pg_catalog.pg_constraint pkc
    on pkc.conindid = c.conindid
    and pkc.conrelid = c.confrelid
    and pkc.contype in ('p', 'u')
where pt.relname = @{nameof(Query.TableName)} and pns.nspname = @{nameof(Query.SchemaName)}
    -- since PG 11 a foreign key declared on a partitioned table is cloned onto each of its
    -- partitions, with the same name and a non-zero conparentid. Only the constraint as declared is
    -- wanted, whether it was declared on a partitioned table or on a partition of its own accord.
    and c.conparentid = 0
""";
}