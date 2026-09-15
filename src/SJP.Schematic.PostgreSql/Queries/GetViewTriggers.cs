using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetViewTriggers
{
    // The shape of a trigger is the same whether it is attached to a table or a view, so the rows are
    // described by GetTableTriggers.Result and mapped by the same code.
    internal sealed record Query : ISqlQuery<GetTableTriggers.Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    // Only a plain view can carry triggers, all of them INSTEAD OF; a materialized view cannot, so
    // relkind 'm' is admitted purely so that the same query serves both view providers. Apart from the
    // relkind filter the query matches GetTableTriggers.Sql, which describes how its columns are built.
    internal const string Sql = $"""

select
    tr.tgname as "{nameof(GetTableTriggers.Result.TriggerName)}",
    tr.tgenabled as "{nameof(GetTableTriggers.Result.EnabledFlag)}",
    def.definition as "{nameof(GetTableTriggers.Result.Definition)}",
    case tr.tgtype & 66 when 2 then 'BEFORE' when 64 then 'INSTEAD OF' else 'AFTER' end as "{nameof(GetTableTriggers.Result.Timing)}",
    case tr.tgtype & 1 when 1 then 'ROW' else 'STATEMENT' end as "{nameof(GetTableTriggers.Result.Granularity)}",
    substring(def.definition from ' WHEN \((.+)\) EXECUTE ') as "{nameof(GetTableTriggers.Result.Condition)}",
    (
        select pg_catalog.array_agg(a.attname::text order by a.attnum)
        from pg_catalog.pg_attribute a
        where a.attrelid = tr.tgrelid and a.attnum = any(tr.tgattr::int2[])
    ) as "{nameof(GetTableTriggers.Result.UpdateColumns)}",
    pg_catalog.array_remove(array[
        case when tr.tgtype & 4 <> 0 then 'INSERT' end,
        case when tr.tgtype & 8 <> 0 then 'DELETE' end,
        case when tr.tgtype & 16 <> 0 then 'UPDATE' end,
        case when tr.tgtype & 32 <> 0 then 'TRUNCATE' end
    ], null) as "{nameof(GetTableTriggers.Result.TriggerEvents)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on tr.tgrelid = t.oid and not tr.tgisinternal
cross join lateral (select pg_catalog.pg_get_triggerdef(tr.oid) as definition offset 0) def
where t.relkind in ('v', 'm')
    and (tr.tgtype & 60) <> 0
    and t.relname = @{nameof(Query.ViewName)}
    and ns.nspname = @{nameof(Query.SchemaName)}
""";
}
