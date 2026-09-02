using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// The <c>pg_proc</c> row's OID, which identifies one overload. Also the order the
        /// overloads are reported in.
        /// </summary>
        public required uint RoutineOid { get; init; }

        public required string? Definition { get; init; }

        /// <summary>
        /// <c>pg_proc.prokind</c>: <c>f</c> for a function, <c>p</c> for a procedure,
        /// <c>a</c> for an aggregate and <c>w</c> for a window function.
        /// </summary>
        public required string RoutineKind { get; init; }

        public required string? Language { get; init; }

        /// <summary>
        /// The name of the type the overload returns, unqualified. <c>void</c> for a procedure.
        /// </summary>
        public required string? ReturnTypeName { get; init; }

        public required string? ReturnTypeSchema { get; init; }
    }

    // one row per overload. pg_get_functiondef() raises an error for aggregates, so it is never
    // called for them; their definition is rebuilt from pg_aggregate instead.
    internal const string Sql = $"""

select
    p.oid as "{nameof(Result.RoutineOid)}",
    coalesce(fd.definition, agg.definition) as "{nameof(Result.Definition)}",
    p.prokind::text as "{nameof(Result.RoutineKind)}",
    l.lanname as "{nameof(Result.Language)}",
    rt.typname as "{nameof(Result.ReturnTypeName)}",
    rn.nspname as "{nameof(Result.ReturnTypeSchema)}"
from pg_catalog.pg_proc p
inner join pg_catalog.pg_namespace n on n.oid = p.pronamespace
left join pg_catalog.pg_language l on l.oid = p.prolang
left join pg_catalog.pg_type rt on rt.oid = p.prorettype
left join pg_catalog.pg_namespace rn on rn.oid = rt.typnamespace
left join lateral (
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
order by p.oid
""";
}
