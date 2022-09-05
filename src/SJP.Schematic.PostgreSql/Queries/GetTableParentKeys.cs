namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableParentKeys
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ChildKeyName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public string ParentSchemaName { get; init; } = default!;

        public string ParentTableName { get; init; } = default!;

        public int ConstraintColumnId { get; init; }

        public string ParentKeyName { get; init; } = default!;

        public string ParentKeyType { get; init; } = default!;

        public string DeleteAction { get; init; } = default!;

        public string UpdateAction { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    c.conname as ""{nameof(Result.ChildKeyName)}"",
    tc.attname as ""{nameof(Result.ColumnName)}"",
    child_cols.con_index as ""{nameof(Result.ConstraintColumnId)}"",
    pns.nspname as ""{nameof(Result.ParentSchemaName)}"",
    pt.relname as ""{nameof(Result.ParentTableName)}"",
    pkc.contype as ""{nameof(Result.ParentKeyType)}"",
    pkc.conname as ""{nameof(Result.ParentKeyName)}"",
    c.confupdtype as ""{nameof(Result.UpdateAction)}"",
    c.confdeltype as ""{nameof(Result.DeleteAction)}""
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid and c.contype = 'f'
inner join pg_catalog.pg_attribute tc on tc.attrelid = t.oid and tc.attnum = any(c.conkey)
inner join pg_catalog.unnest(c.conkey) with ordinality as child_cols(col_index, con_index) on child_cols.col_index = tc.attnum
inner join pg_catalog.pg_class pt on pt.oid = c.confrelid
inner join pg_catalog.pg_namespace pns on pns.oid = pt.relnamespace
left join pg_catalog.pg_depend d1  -- find constraint's dependency on an index
    on d1.objid = c.oid
    and d1.classid = 'pg_constraint'::regclass
    and d1.refclassid = 'pg_class'::regclass
    and d1.refobjsubid = 0
left join pg_catalog.pg_depend d2  -- find pkey/unique constraint for that index
    on d2.refclassid = 'pg_constraint'::regclass
    and d2.classid = 'pg_class'::regclass
    and d2.objid = d1.refobjid
    and d2.objsubid = 0
    and d2.deptype = 'i'
left join pg_catalog.pg_constraint pkc on pkc.oid = d2.refobjid
    and pkc.contype in ('p', 'u')
    and pkc.conrelid = c.confrelid
where t.relname = @{nameof(Query.TableName)} and ns.nspname = @{nameof(Query.SchemaName)}";
}