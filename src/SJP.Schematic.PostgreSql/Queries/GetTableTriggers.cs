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

        public required string Granularity { get; init; }

        public required string? Condition { get; init; }

        public required string[]? UpdateColumns { get; init; }

        public required string[] TriggerEvents { get; init; }

        public required string EnabledFlag { get; init; }
    }

    // One row is returned per trigger. The WHEN clause is stored in pg_trigger.tgqual as a node tree
    // whose OLD/NEW references pg_get_expr() cannot deparse, so it is recovered from the rendered
    // definition instead; pg_get_triggerdef() always emits the clause as ' WHEN (...) EXECUTE ' when
    // one is present. The definition is rendered once in a lateral subquery, where 'offset 0' stops
    // the planner from inlining it back into a separate call for each reference.
    // Triggers with none of the INSERT (4), DELETE (8), UPDATE (16) or TRUNCATE (32) bits set are left out.
    internal const string Sql = $"""

select
    tr.tgname as "{nameof(Result.TriggerName)}",
    tr.tgenabled as "{nameof(Result.EnabledFlag)}",
    def.definition as "{nameof(Result.Definition)}",
    case tr.tgtype & 66 when 2 then 'BEFORE' when 64 then 'INSTEAD OF' else 'AFTER' end as "{nameof(Result.Timing)}",
    case tr.tgtype & 1 when 1 then 'ROW' else 'STATEMENT' end as "{nameof(Result.Granularity)}",
    substring(def.definition from ' WHEN \((.+)\) EXECUTE ') as "{nameof(Result.Condition)}",
    (
        select pg_catalog.array_agg(a.attname::text order by a.attnum)
        from pg_catalog.pg_attribute a
        where a.attrelid = tr.tgrelid and a.attnum = any(tr.tgattr::int2[])
    ) as "{nameof(Result.UpdateColumns)}",
    pg_catalog.array_remove(array[
        case when tr.tgtype & 4 <> 0 then 'INSERT' end,
        case when tr.tgtype & 8 <> 0 then 'DELETE' end,
        case when tr.tgtype & 16 <> 0 then 'UPDATE' end,
        case when tr.tgtype & 32 <> 0 then 'TRUNCATE' end
    ], null) as "{nameof(Result.TriggerEvents)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on tr.tgrelid = t.oid and not tr.tgisinternal
cross join lateral (select pg_catalog.pg_get_triggerdef(tr.oid) as definition offset 0) def
where t.relkind in ('r', 'p')
    and (tr.tgtype & 60) <> 0
    and t.relname = @{nameof(Query.TableName)}
    and ns.nspname = @{nameof(Query.SchemaName)}
""";
}
