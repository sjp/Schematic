using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableTriggers
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string TriggerName { get; init; }

        public required string Definition { get; init; }

        public required string Timing { get; init; }

        public required string TriggerEvent { get; init; }

        public required string EnabledFlag { get; init; }
    }

    internal const string Sql = $"""

select
    tr.tgname as "{nameof(Result.TriggerName)}",
    tr.tgenabled as "{nameof(Result.EnabledFlag)}",
    pg_catalog.pg_get_triggerdef(tr.oid) as "{nameof(Result.Definition)}",
    case tr.tgtype & 66 when 2 then 'BEFORE' when 64 then 'INSTEAD OF' else 'AFTER' end as "{nameof(Result.Timing)}",
    ev.event_name as "{nameof(Result.TriggerEvent)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on tr.tgrelid = t.oid and not tr.tgisinternal
cross join (values (4, 'INSERT'), (8, 'DELETE'), (16, 'UPDATE')) as ev(mask, event_name)
where t.relkind in ('r', 'p')
    and (tr.tgtype & ev.mask) <> 0
    and t.relname = @{nameof(Query.TableName)}
    and ns.nspname = @{nameof(Query.SchemaName)}
""";
}