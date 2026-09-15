namespace SJP.Schematic.PostgreSql.Queries;

/// <summary>
/// The parts of a column query that are the same whether the columns belong to a table, a view or a
/// materialized view, so that the three queries describe a column's type identically.
/// </summary>
/// <remarks>
/// This follows the definition of <c>information_schema.columns</c>, reading <c>pg_catalog</c>
/// directly instead. That view matches every relation kind and exposes nothing but names, so going
/// through it means joining the catalog back by name to learn anything it leaves out. The length,
/// precision and scale are computed by the same <c>information_schema._pg_*</c> helper functions that
/// view uses, which ship with every PostgreSQL installation.
/// </remarks>
internal static class ColumnCatalogSql
{
    /// <summary>
    /// The select list entries for every <see cref="IColumnCatalogRow"/> member, without a trailing
    /// comma. Refers to the aliases introduced by <see cref="From"/>.
    /// </summary>
    internal const string SelectList = $"""
    a.attname as "{nameof(IColumnCatalogRow.ColumnName)}",
    a.attnum as "{nameof(IColumnCatalogRow.OrdinalPosition)}",
    case when a.attgenerated = '' then pg_catalog.pg_get_expr(ad.adbin, ad.adrelid) end as "{nameof(IColumnCatalogRow.ColumnDefault)}",
    case when a.attnotnull or (t.typtype = 'd' and t.typnotnull) then 'NO' else 'YES' end as "{nameof(IColumnCatalogRow.IsNullable)}",

    case when t.typtype = 'd' then
        case when bt.typelem <> 0 and bt.typlen = -1 then 'ARRAY'
             when nbt.nspname = 'pg_catalog' then pg_catalog.format_type(t.typbasetype, null)
             else 'USER-DEFINED' end
    else
        case when t.typelem <> 0 and t.typlen = -1 then 'ARRAY'
             when nt.nspname = 'pg_catalog' then pg_catalog.format_type(a.atttypid, null)
             else 'USER-DEFINED' end
    end as "{nameof(IColumnCatalogRow.DataType)}",

    information_schema._pg_char_max_length(tt.typid, tt.typmod) as "{nameof(IColumnCatalogRow.CharacterMaximumLength)}",
    information_schema._pg_numeric_precision(tt.typid, tt.typmod) as "{nameof(IColumnCatalogRow.NumericPrecision)}",
    information_schema._pg_numeric_precision_radix(tt.typid, tt.typmod) as "{nameof(IColumnCatalogRow.NumericPrecisionRadix)}",
    information_schema._pg_numeric_scale(tt.typid, tt.typmod) as "{nameof(IColumnCatalogRow.NumericScale)}",
    information_schema._pg_datetime_precision(tt.typid, tt.typmod) as "{nameof(IColumnCatalogRow.DatetimePrecision)}",

    case when nco.nspname is not null then pg_catalog.current_database() end as "{nameof(IColumnCatalogRow.CollationCatalog)}",
    nco.nspname as "{nameof(IColumnCatalogRow.CollationSchema)}",
    co.collname as "{nameof(IColumnCatalogRow.CollationName)}",

    case when t.typtype = 'd' then nt.nspname end as "{nameof(IColumnCatalogRow.DomainSchema)}",
    case when t.typtype = 'd' then t.typname end as "{nameof(IColumnCatalogRow.DomainName)}",

    coalesce(nbt.nspname, nt.nspname) as "{nameof(IColumnCatalogRow.UdtSchema)}",
    coalesce(bt.typname, t.typname) as "{nameof(IColumnCatalogRow.UdtName)}",

    coalesce(bt.typtype, t.typtype)::text as "{nameof(IColumnCatalogRow.TypeKind)}",
    elem_ns.nspname as "{nameof(IColumnCatalogRow.ElementTypeSchema)}",
    elem.typname as "{nameof(IColumnCatalogRow.ElementTypeName)}",
    elem.typtype::text as "{nameof(IColumnCatalogRow.ElementTypeKind)}",
    lbl.labels as "{nameof(IColumnCatalogRow.EnumLabels)}"
""";

    /// <summary>
    /// The <c>from</c> clause, introducing <c>a</c> (<c>pg_attribute</c>), <c>c</c> (<c>pg_class</c>)
    /// and <c>nc</c> (its <c>pg_namespace</c>) for the enclosing query to filter and extend.
    /// </summary>
    internal const string From = """
from pg_catalog.pg_attribute a
    left join pg_catalog.pg_attrdef ad on ad.adrelid = a.attrelid and ad.adnum = a.attnum
    inner join pg_catalog.pg_class c on c.oid = a.attrelid
    inner join pg_catalog.pg_namespace nc on nc.oid = c.relnamespace
    inner join pg_catalog.pg_type t on t.oid = a.atttypid
    inner join pg_catalog.pg_namespace nt on nt.oid = t.typnamespace
    -- a column over a domain is described by the domain's base type
    left join (pg_catalog.pg_type bt inner join pg_catalog.pg_namespace nbt on nbt.oid = bt.typnamespace)
        on t.typtype = 'd' and t.typbasetype = bt.oid
    left join (pg_catalog.pg_collation co inner join pg_catalog.pg_namespace nco on nco.oid = co.collnamespace)
        on a.attcollation = co.oid and (nco.nspname, co.collname) <> ('pg_catalog', 'default')
    cross join lateral (
        select
            information_schema._pg_truetypid(a, t) as typid,
            information_schema._pg_truetypmod(a, t) as typmod
    ) tt
    -- the type name alone says nothing about what kind of type it is, what an array holds, or which
    -- labels an enum permits
    left join pg_catalog.pg_type elem on elem.oid = coalesce(bt.typelem, t.typelem)
    left join pg_catalog.pg_namespace elem_ns on elem_ns.oid = elem.typnamespace
    left join lateral (
        select array_agg(en.enumlabel::text order by en.enumsortorder) as labels
        from pg_catalog.pg_enum en
        where en.enumtypid = case
            when coalesce(bt.typtype, t.typtype) = 'e' then coalesce(bt.oid, t.oid)
            when elem.typtype = 'e' then elem.oid end
    ) lbl on true
""";

    /// <summary>
    /// The <c>where</c> predicates, without the <c>where</c> keyword, that keep the live user columns
    /// the current user is allowed to see. The enclosing query adds the relation kind and name.
    /// </summary>
    internal const string VisibleColumnsPredicate = """
not pg_catalog.pg_is_other_temp_schema(nc.oid)
    and a.attnum > 0 and not a.attisdropped
    and (pg_catalog.pg_has_role(c.relowner, 'USAGE')
        or pg_catalog.has_column_privilege(c.oid, a.attnum, 'SELECT, INSERT, UPDATE, REFERENCES'))
""";
}
