using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? ObjectType { get; init; }

        public required string? ObjectName { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

with rel as materialized (
    select t.oid as reloid, t.relname
    from pg_catalog.pg_class t
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    where t.relkind in ('r', 'p')
        and ns.nspname = @{nameof(Query.SchemaName)}
        and t.relname = @{nameof(Query.TableName)}
        and ns.nspname not in ('pg_catalog', 'information_schema')
)
-- table
select
    'TABLE' as "{nameof(Result.ObjectType)}",
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

union all

-- check / foreign key / unique / primary key constraints
select
    case c.contype
        when 'c' then 'CHECK'
        when 'f' then 'FOREIGN KEY'
        when 'u' then 'UNIQUE'
        when 'p' then 'PRIMARY'
    end as "{nameof(Result.ObjectType)}",
    c.conname as "{nameof(Result.ObjectName)}",
    d.description as "{nameof(Result.Comment)}"
from rel r
inner join pg_catalog.pg_constraint c
    on c.conrelid = r.reloid and c.contype in ('c', 'f', 'u', 'p')
left join pg_catalog.pg_description d
    on d.objoid = c.oid
    and d.classoid = 'pg_catalog.pg_constraint'::regclass
    and d.objsubid = 0

union all

-- indexes
select
    'INDEX' as "{nameof(Result.ObjectType)}",
    ci.relname as "{nameof(Result.ObjectName)}",
    d.description as "{nameof(Result.Comment)}"
from rel r
inner join pg_catalog.pg_index i on i.indrelid = r.reloid and not i.indisprimary
inner join pg_catalog.pg_class ci on ci.oid = i.indexrelid and ci.relkind in ('i', 'I')
left join pg_catalog.pg_description d
    on d.objoid = i.indexrelid
    and d.classoid = 'pg_catalog.pg_class'::regclass
    and d.objsubid = 0

union all

-- triggers
select
    'TRIGGER' as "{nameof(Result.ObjectType)}",
    tr.tgname as "{nameof(Result.ObjectName)}",
    d.description as "{nameof(Result.Comment)}"
from rel r
inner join pg_catalog.pg_trigger tr on tr.tgrelid = r.reloid and not tr.tgisinternal
left join pg_catalog.pg_description d
    on d.objoid = tr.oid
    and d.classoid = 'pg_catalog.pg_trigger'::regclass
    and d.objsubid = 0

""";
}