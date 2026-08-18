using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = $"""

select pg_catalog.string_agg(r.definition, E'\n\n' order by r.oid)
from (
    select
        p.oid,
        coalesce(fd.definition, agg.definition) as definition
    from pg_catalog.pg_proc p
    inner join pg_catalog.pg_namespace n on n.oid = p.pronamespace
    left join lateral (
        -- pg_get_functiondef() raises an error for aggregates, so never call it for them
        select pg_catalog.pg_get_functiondef(p.oid) as definition
        where p.prokind <> 'a'
    ) fd on true
    left join lateral (
        select pg_catalog.format(
            E'CREATE AGGREGATE %s.%s(%s) (\n    SFUNC = %s,\n    STYPE = %s%s%s\n);',
            pg_catalog.quote_ident(n.nspname),
            pg_catalog.quote_ident(p.proname),
            pg_catalog.pg_get_function_arguments(p.oid),
            a.aggtransfn::pg_catalog.regprocedure,
            pg_catalog.format_type(a.aggtranstype, null),
            case when a.aggfinalfn <> 0
                 then E',\n    FINALFUNC = ' || a.aggfinalfn::pg_catalog.regprocedure::text
                 else '' end,
            case when a.agginitval is not null
                 then E',\n    INITCOND = ' || pg_catalog.quote_literal(a.agginitval)
                 else '' end
        ) as definition
        from pg_catalog.pg_aggregate a
        where a.aggfnoid = p.oid
    ) agg on true
    where n.nspname = @{nameof(Query.SchemaName)} and p.proname = @{nameof(Query.RoutineName)}
        and n.nspname not in ('pg_catalog', 'information_schema')
) r
limit 1
""";
}
