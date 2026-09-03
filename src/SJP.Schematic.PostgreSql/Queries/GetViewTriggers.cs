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
    // relkind 'm' is admitted purely so that the same query serves both view providers.
    internal const string Sql = $"""

select
    tr.tgname as "{nameof(GetTableTriggers.Result.TriggerName)}",
    tr.tgenabled as "{nameof(GetTableTriggers.Result.EnabledFlag)}",
    pg_catalog.pg_get_triggerdef(tr.oid) as "{nameof(GetTableTriggers.Result.Definition)}",
    case tr.tgtype & 66 when 2 then 'BEFORE' when 64 then 'INSTEAD OF' else 'AFTER' end as "{nameof(GetTableTriggers.Result.Timing)}",
    case tr.tgtype & 1 when 1 then 'ROW' else 'STATEMENT' end as "{nameof(GetTableTriggers.Result.Granularity)}",
    substring(pg_catalog.pg_get_triggerdef(tr.oid) from ' WHEN \((.+)\) EXECUTE ') as "{nameof(GetTableTriggers.Result.Condition)}",
    (
        select pg_catalog.array_agg(a.attname::text order by a.attnum)
        from pg_catalog.pg_attribute a
        where a.attrelid = tr.tgrelid and a.attnum = any(tr.tgattr::int2[])
    ) as "{nameof(GetTableTriggers.Result.UpdateColumns)}",
    ev.event_name as "{nameof(GetTableTriggers.Result.TriggerEvent)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on tr.tgrelid = t.oid and not tr.tgisinternal
cross join (values (4, 'INSERT'), (8, 'DELETE'), (16, 'UPDATE'), (32, 'TRUNCATE')) as ev(mask, event_name)
where t.relkind in ('v', 'm')
    and (tr.tgtype & ev.mask) <> 0
    and t.relname = @{nameof(Query.ViewName)}
    and ns.nspname = @{nameof(Query.SchemaName)}
""";
}
