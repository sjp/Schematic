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
    pkc.contype as "{nameof(Result.ParentKeyType)}",
    pkc.conname as "{nameof(Result.ParentKeyName)}",
    c.confupdtype as "{nameof(Result.UpdateAction)}",
    c.confdeltype as "{nameof(Result.DeleteAction)}"
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid and c.contype = 'f'
inner join pg_catalog.pg_class pt on pt.oid = c.confrelid
inner join pg_catalog.pg_namespace pns on pns.oid = pt.relnamespace
left join pg_catalog.pg_depend d1  -- find constraint's dependency on an index
    on d1.objid = c.oid
    and d1.classid = 'pg_constraint'::regclass
    and d1.refclassid = 'pg_class'::regclass
    and d1.refobjsubid = 0
left join pg_catalog.pg_depend d2  -- find pkey/unique constraint for that index
    on d2.refclassid = 'pg_constraint'::regclass
    and d2.classid = 'pg_class'::regclass
    and d2.objid = d1.refobjid
    and d2.objsubid = 0
    and d2.deptype = 'i'
left join pg_catalog.pg_constraint pkc on pkc.oid = d2.refobjid
    and pkc.contype in ('p', 'u')
    and pkc.conrelid = c.confrelid
where pt.relname = @{nameof(Query.TableName)} and pns.nspname = @{nameof(Query.SchemaName)}
""";
}