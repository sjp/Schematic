using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetRoutineParameters
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// The OID of the overload this parameter belongs to, joining back to
        /// <see cref="GetRoutineDefinition.Result.RoutineOid"/>.
        /// </summary>
        public required uint RoutineOid { get; init; }

        public required int Ordinal { get; init; }

        public required string? ParameterName { get; init; }

        /// <summary>
        /// <c>pg_proc.proargmodes</c>: <c>i</c> in, <c>o</c> out, <c>b</c> in/out and
        /// <c>v</c> variadic. Table result columns (<c>t</c>) are excluded by the query, as they
        /// describe the return value rather than the signature.
        /// </summary>
        public required string ParameterMode { get; init; }

        public required string TypeName { get; init; }

        public required string? TypeSchema { get; init; }

        public required string? DefaultValue { get; init; }
    }

    // proallargtypes is only populated when the routine has an argument that is not plain IN,
    // so it falls back to proargtypes. proargnames indexes over the same array either way.
    //
    // pg_get_function_arg_default() indexes over input arguments alone, which only lines up with
    // the array position when every argument is an input one - so the default is left unread
    // rather than misattributed when the routine mixes argument modes.
    internal const string Sql = $"""

select
    p.oid as "{nameof(Result.RoutineOid)}",
    args.arg_position::int as "{nameof(Result.Ordinal)}",
    p.proargnames[args.arg_position::int] as "{nameof(Result.ParameterName)}",
    coalesce(p.proargmodes[args.arg_position::int], 'i')::text as "{nameof(Result.ParameterMode)}",
    t.typname as "{nameof(Result.TypeName)}",
    tn.nspname as "{nameof(Result.TypeSchema)}",
    case when p.proallargtypes is null
         then pg_catalog.pg_get_function_arg_default(p.oid, args.arg_position::int)
    end as "{nameof(Result.DefaultValue)}"
from pg_catalog.pg_proc p
inner join pg_catalog.pg_namespace n on n.oid = p.pronamespace
cross join unnest(coalesce(p.proallargtypes, p.proargtypes::oid[])) with ordinality as args(arg_type, arg_position)
inner join pg_catalog.pg_type t on t.oid = args.arg_type
left join pg_catalog.pg_namespace tn on tn.oid = t.typnamespace
where n.nspname = @{nameof(Query.SchemaName)} and p.proname = @{nameof(Query.RoutineName)}
    and n.nspname not in ('pg_catalog', 'information_schema')
    and coalesce(p.proargmodes[args.arg_position::int], 'i') <> 't'
order by p.oid, args.arg_position
""";
}
