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
        /// The name of the type the overload returns, unqualified. <c>void</c> for a procedure,
        /// unless it declares output parameters, then it describes those instead of a return
        /// value, and is ignored.
        /// </summary>
        public required string? ReturnTypeName { get; init; }

        public required string? ReturnTypeSchema { get; init; }

        /// <summary>
        /// The 1-based position of each parameter in the overload's signature. Every
        /// <c>Parameter*</c> array lines up with this one; all of them are <see langword="null" />
        /// when the overload takes no parameters.
        /// </summary>
        public required int[]? ParameterOrdinals { get; init; }

        public required string?[]? ParameterNames { get; init; }

        /// <summary>
        /// <c>pg_proc.proargmodes</c>: <c>i</c> in, <c>o</c> out, <c>b</c> in/out and
        /// <c>v</c> variadic. Table result columns (<c>t</c>) are excluded by the query, as they
        /// describe the return value rather than the signature.
        /// </summary>
        public required string[]? ParameterModes { get; init; }

        public required string[]? ParameterTypeNames { get; init; }

        public required string?[]? ParameterTypeSchemas { get; init; }

        public required string?[]? ParameterDefaults { get; init; }
    }

    // one row per overload. pg_get_functiondef() raises an error for aggregates, so it is never
    // called for them; their definition is rebuilt from pg_aggregate instead. This must be a CASE:
    // the planner may evaluate a lateral subquery's select list before its WHERE clause, but a
    // CASE only evaluates the branch that applies.
    //
    // Each overload's parameters come back as parallel arrays, ordered by position. proallargtypes
    // is only populated when the routine has an argument that is not plain IN, so it falls back
    // to proargtypes; proargnames and proargmodes index over the same array either way.
    // pg_get_function_arg_default() also takes a position over all arguments, returning null
    // for one that is not an input.
    internal const string Sql = $"""
select
    p.oid as "{nameof(Result.RoutineOid)}",
    case
        when p.prokind = 'a' then (
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
            )
            from pg_catalog.pg_aggregate a
            where a.aggfnoid = p.oid)
        else pg_catalog.pg_get_functiondef(p.oid)
    end as "{nameof(Result.Definition)}",
    p.prokind::text as "{nameof(Result.RoutineKind)}",
    l.lanname as "{nameof(Result.Language)}",
    rt.typname as "{nameof(Result.ReturnTypeName)}",
    rn.nspname as "{nameof(Result.ReturnTypeSchema)}",
    prm.ordinals as "{nameof(Result.ParameterOrdinals)}",
    prm.names as "{nameof(Result.ParameterNames)}",
    prm.modes as "{nameof(Result.ParameterModes)}",
    prm.type_names as "{nameof(Result.ParameterTypeNames)}",
    prm.type_schemas as "{nameof(Result.ParameterTypeSchemas)}",
    prm.defaults as "{nameof(Result.ParameterDefaults)}"
from pg_catalog.pg_proc p
inner join pg_catalog.pg_namespace n on n.oid = p.pronamespace
left join pg_catalog.pg_language l on l.oid = p.prolang
left join pg_catalog.pg_type rt on rt.oid = p.prorettype
left join pg_catalog.pg_namespace rn on rn.oid = rt.typnamespace
cross join lateral (
    select
        array_agg(args.arg_position::int order by args.arg_position) as ordinals,
        array_agg(p.proargnames[args.arg_position::int] order by args.arg_position) as names,
        array_agg(coalesce(p.proargmodes[args.arg_position::int], 'i')::text order by args.arg_position) as modes,
        array_agg(t.typname::text order by args.arg_position) as type_names,
        array_agg(tn.nspname::text order by args.arg_position) as type_schemas,
        array_agg(pg_catalog.pg_get_function_arg_default(p.oid, args.arg_position::int) order by args.arg_position) as defaults
    from unnest(coalesce(p.proallargtypes, p.proargtypes::oid[])) with ordinality as args(arg_type, arg_position)
    inner join pg_catalog.pg_type t on t.oid = args.arg_type
    left join pg_catalog.pg_namespace tn on tn.oid = t.typnamespace
    where coalesce(p.proargmodes[args.arg_position::int], 'i') <> 't'
) prm
where n.nspname = @{nameof(Query.SchemaName)} and p.proname = @{nameof(Query.RoutineName)}
    and n.nspname not in ('pg_catalog', 'information_schema')
order by p.oid
""";
}
