using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetViewComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        public required string? ObjectType { get; init; }

        public required string? ObjectName { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

with rel as materialized (
    select c.oid as reloid, c.relname
    from pg_catalog.pg_class c
    inner join pg_catalog.pg_namespace n on n.oid = c.relnamespace
    where c.relkind = 'v'
        and n.nspname = @{nameof(Query.SchemaName)}
        and c.relname = @{nameof(Query.ViewName)}
        and n.nspname not in ('pg_catalog', 'information_schema')
)
-- view
select
    'VIEW' as "{nameof(Result.ObjectType)}",
    r.relname as "{nameof(Result.ObjectName)}",
    d.description as "{nameof(Result.Comment)}"
from rel r
left join pg_catalog.pg_description d
    on d.objoid = r.reloid
    and d.classoid = 'pg_catalog.pg_class'::regclass
    and d.objsubid = 0

union all

-- columns
select
    'COLUMN' as "{nameof(Result.ObjectType)}",
    a.attname as "{nameof(Result.ObjectName)}",
    d.description as "{nameof(Result.Comment)}"
from rel r
inner join pg_catalog.pg_attribute a
    on a.attrelid = r.reloid and a.attnum > 0 and not a.attisdropped
left join pg_catalog.pg_description d
    on d.objoid = r.reloid
    and d.classoid = 'pg_catalog.pg_class'::regclass
    and d.objsubid = a.attnum

""";
}