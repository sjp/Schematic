using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetUserDefinedTypeComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal sealed record Result
    {
        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

select
    d.description as "{nameof(Result.Comment)}"
from pg_catalog.pg_type t
inner join pg_catalog.pg_namespace n on n.oid = t.typnamespace
left join pg_catalog.pg_description d
    on d.objoid = t.oid
    and d.classoid = 'pg_catalog.pg_type'::regclass
    and d.objsubid = 0
where n.nspname = @{nameof(Query.SchemaName)} and t.typname = @{nameof(Query.TypeName)}
    and n.nspname not in ('pg_catalog', 'information_schema', 'pg_toast')
limit 1
""";
}
