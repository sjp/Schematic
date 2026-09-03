namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllSchemaComments
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

select
    n.nspname as "{nameof(Result.SchemaName)}",
    d.description as "{nameof(Result.Comment)}"
from pg_catalog.pg_namespace n
left join pg_catalog.pg_description d
    on d.objoid = n.oid
    and d.classoid = 'pg_catalog.pg_namespace'::regclass
    and d.objsubid = 0
order by n.nspname
""";
}
