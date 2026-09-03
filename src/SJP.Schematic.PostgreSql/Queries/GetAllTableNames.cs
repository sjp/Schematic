namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllTableNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal const string Sql = $"""

select
    ns.nspname as "{nameof(Result.SchemaName)}",
    t.relname as "{nameof(Result.TableName)}"
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
where t.relkind in ('r', 'p')
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and not pg_catalog.pg_is_other_temp_schema(ns.oid)
order by ns.nspname, t.relname
""";
}