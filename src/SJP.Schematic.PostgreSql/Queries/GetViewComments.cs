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

-- view
select
    'VIEW' as "{nameof(Result.ObjectType)}",
    c.relname as "{nameof(Result.ObjectName)}",
    d.description as "{nameof(Result.Comment)}"
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
left join pg_catalog.pg_description d on c.oid = d.objoid and d.objsubid = 0
where n.nspname = @{nameof(Query.SchemaName)} and c.relname = @{nameof(Query.ViewName)}
    and c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select
    'COLUMN' as "{nameof(Result.ObjectType)}",
    a.attname as "{nameof(Result.ObjectName)}",
    d.description as "{nameof(Result.Comment)}"
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
inner join pg_catalog.pg_attribute a on a.attrelid = c.oid
left join pg_description d on c.oid = d.objoid and a.attnum = d.objsubid
where n.nspname = @{nameof(Query.SchemaName)} and c.relname = @{nameof(Query.ViewName)}
    and c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped

""";
}