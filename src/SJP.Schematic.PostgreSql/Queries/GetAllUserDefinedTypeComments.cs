namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllUserDefinedTypeComments
{
    internal sealed record Result
    {
        public required string? SchemaName { get; init; }

        public required string? TypeName { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

select
    n.nspname as "{nameof(Result.SchemaName)}",
    t.typname as "{nameof(Result.TypeName)}",
    d.description as "{nameof(Result.Comment)}"
from pg_catalog.pg_type t
inner join pg_catalog.pg_namespace n on n.oid = t.typnamespace
left join pg_catalog.pg_class cls on cls.oid = t.typrelid
left join pg_catalog.pg_description d
    on d.objoid = t.oid
    and d.classoid = 'pg_catalog.pg_type'::regclass
    and d.objsubid = 0
where n.nspname not in ('pg_catalog', 'information_schema', 'pg_toast')
    and t.typtype in ('d', 'e', 'c', 'r')
    and (t.typtype <> 'c' or cls.relkind = 'c')
order by n.nspname, t.typname
""";
}
