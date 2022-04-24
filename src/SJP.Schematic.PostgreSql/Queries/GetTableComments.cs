namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableComments
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? ObjectType { get; init; }

        public string? ObjectName { get; init; }

        public string? Comment { get; init; }
    }

    internal const string Sql = @$"
-- table
select
    'TABLE' as ""{ nameof(Result.ObjectType) }"",
    t.relname as ""{ nameof(Result.ObjectName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
left join pg_catalog.pg_description d on d.objoid = t.oid and d.objsubid = 0
where t.relkind = 'r' and ns.nspname = @{ nameof(Query.SchemaName) } and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select
    'COLUMN' as ""{ nameof(Result.ObjectType) }"",
    a.attname as ""{ nameof(Result.ObjectName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_attribute a on a.attrelid = t.oid
left join pg_description d on t.oid = d.objoid and a.attnum = d.objsubid
where t.relkind = 'r' and ns.nspname = @{ nameof(Query.SchemaName) } and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped

union

-- checks
select
    'CHECK' as ""{ nameof(Result.ObjectType) }"",
    c.conname as ""{ nameof(Result.ObjectName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(Query.SchemaName) } and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'c'

union

-- foreign keys
select
    'FOREIGN KEY' as ""{ nameof(Result.ObjectType) }"",
    c.conname as ""{ nameof(Result.ObjectName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(Query.SchemaName) } and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'f'

union

-- unique keys
select
    'UNIQUE' as ""{ nameof(Result.ObjectType) }"",
    c.conname as ""{ nameof(Result.ObjectName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(Query.SchemaName) } and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'u'

union

-- primary key
select
    'PRIMARY' as ""{ nameof(Result.ObjectType) }"",
    c.conname as ""{ nameof(Result.ObjectName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(Query.SchemaName) } and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'p'

union

-- indexes
select
    'INDEX' as ""{ nameof(Result.ObjectType) }"",
    ci.relname as ""{ nameof(Result.ObjectName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_index i on i.indrelid = t.oid and i.indisprimary = false
inner join pg_catalog.pg_class ci on i.indexrelid = ci.oid and ci.relkind = 'i'
left join pg_catalog.pg_description d on d.objoid = i.indexrelid
where t.relkind = 'r' and ns.nspname = @{ nameof(Query.SchemaName) } and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')

union

-- triggers
select
    'TRIGGER' as ""{ nameof(Result.ObjectType) }"",
    tr.tgname as ""{ nameof(Result.ObjectName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_trigger tr on tr.tgrelid = t.oid and tr.tgisinternal = false
left join pg_catalog.pg_description d on d.objoid = tr.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(Query.SchemaName) } and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
";
}