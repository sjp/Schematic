namespace SJP.Schematic.PostgreSql.Queries;

/// <summary>
/// The parts of a column query that are the same whether the columns belong to a table, a view or a
/// materialized view, so that the three queries describe a column's type identically.
/// </summary>
/// <remarks>
/// <para>
/// This follows the definition of <c>information_schema.columns</c>, reading <c>pg_catalog</c>
/// directly instead. That view matches every relation kind and exposes nothing but names, so going
/// through it means joining the catalog back by name to learn anything it leaves out.
/// </para>
/// <para>
/// The length, precision and scale are computed by the bodies of the <c>information_schema._pg_*</c>
/// helper functions that view uses, written out inline. The helpers are strict SQL functions with
/// non-strict bodies, so PostgreSQL cannot inline them and would otherwise run each one through the
/// SQL function executor for every column. The hard-coded type OIDs are the ones those bodies use.
/// </para>
/// <para>
/// The enclosing query is expected to look its relation up in a subquery aliased <c>c</c>, exposing
/// <c>oid</c> and <c>relowner</c>, and fenced with <c>offset 0</c>: together with the fences in
/// <see cref="Joins"/>, that keeps each part of the query small enough to be planned on its own.
/// As one flat join it holds well over <c>join_collapse_limit</c> relations, and planning it costs
/// several times more than running it.
/// </para>
/// </remarks>
internal static class ColumnCatalogSql
{
    /// <summary>
    /// The select list entries for every <see cref="IColumnCatalogRow"/> member, without a trailing
    /// comma. Refers to the aliases introduced by <see cref="Joins"/>.
    /// </summary>
    internal const string SelectList = $"""
    a.attname as "{nameof(IColumnCatalogRow.ColumnName)}",
    a.attnum as "{nameof(IColumnCatalogRow.OrdinalPosition)}",
    case when a.attgenerated = '' then pg_catalog.pg_get_expr(ad.adbin, ad.adrelid) end as "{nameof(IColumnCatalogRow.ColumnDefault)}",
    case when a.attnotnull or ty.domain_not_null then 'NO' else 'YES' end as "{nameof(IColumnCatalogRow.IsNullable)}",
    ty.data_type as "{nameof(IColumnCatalogRow.DataType)}",
    ty.char_max_length as "{nameof(IColumnCatalogRow.CharacterMaximumLength)}",
    ty.numeric_precision as "{nameof(IColumnCatalogRow.NumericPrecision)}",
    ty.numeric_precision_radix as "{nameof(IColumnCatalogRow.NumericPrecisionRadix)}",
    ty.numeric_scale as "{nameof(IColumnCatalogRow.NumericScale)}",
    ty.datetime_precision as "{nameof(IColumnCatalogRow.DatetimePrecision)}",
    case when coll.nspname is not null then pg_catalog.current_database() end as "{nameof(IColumnCatalogRow.CollationCatalog)}",
    coll.nspname as "{nameof(IColumnCatalogRow.CollationSchema)}",
    coll.collname as "{nameof(IColumnCatalogRow.CollationName)}",
    ty.domain_schema as "{nameof(IColumnCatalogRow.DomainSchema)}",
    ty.domain_name as "{nameof(IColumnCatalogRow.DomainName)}",
    ty.udt_schema as "{nameof(IColumnCatalogRow.UdtSchema)}",
    ty.udt_name as "{nameof(IColumnCatalogRow.UdtName)}",
    ty.type_kind as "{nameof(IColumnCatalogRow.TypeKind)}",
    ty.element_type_schema as "{nameof(IColumnCatalogRow.ElementTypeSchema)}",
    ty.element_type_name as "{nameof(IColumnCatalogRow.ElementTypeName)}",
    ty.element_type_kind as "{nameof(IColumnCatalogRow.ElementTypeKind)}",
    ty.labels as "{nameof(IColumnCatalogRow.EnumLabels)}"
""";

    // Each "offset 0" below keeps the planner from flattening its subquery into the enclosing join
    // search, so that the subquery is planned on its own. Removing one can make the query several
    // times slower to plan without changing its result.
    //
    // ty describes the column's type. A column over a domain is described by the domain's base type
    // (bt), as information_schema does; tt holds what information_schema._pg_truetypid() and
    // _pg_truetypmod() would return. The type name alone says nothing about what kind of type it is,
    // what an array holds, or which labels an enum permits, so elem adds the element type and the
    // enum labels of whichever of the two is an enum.
    //
    // coll leaves out the default collation, which information_schema does not report either.

    /// <summary>
    /// The joins that follow the enclosing query's relation subquery <c>c</c>, introducing <c>a</c>
    /// (<c>pg_attribute</c>), <c>ad</c> (<c>pg_attrdef</c>), <c>ty</c> (the column's type) and
    /// <c>coll</c> (its collation) for the enclosing query to filter and extend.
    /// </summary>
    internal const string Joins = """
inner join pg_catalog.pg_attribute a on a.attrelid = c.oid
left join pg_catalog.pg_attrdef ad on ad.adrelid = a.attrelid and ad.adnum = a.attnum
cross join lateral (
    select
        t.typtype = 'd' and t.typnotnull as domain_not_null,
        case when t.typtype = 'd' then
            case when bt.typelem <> 0 and bt.typlen = -1 then 'ARRAY'
                 when nbt.nspname = 'pg_catalog' then pg_catalog.format_type(t.typbasetype, null)
                 else 'USER-DEFINED' end
        else
            case when t.typelem <> 0 and t.typlen = -1 then 'ARRAY'
                 when nt.nspname = 'pg_catalog' then pg_catalog.format_type(a.atttypid, null)
                 else 'USER-DEFINED' end
        end as data_type,
        case when tt.typmod = -1 then null
             when tt.typid in (1042, 1043) then tt.typmod - 4
             when tt.typid in (1560, 1562) then tt.typmod end as char_max_length,
        case tt.typid
            when 21 then 16
            when 23 then 32
            when 20 then 64
            when 1700 then case when tt.typmod = -1 then null else ((tt.typmod - 4) >> 16) & 65535 end
            when 700 then 24
            when 701 then 53 end as numeric_precision,
        case when tt.typid in (21, 23, 20, 700, 701) then 2
             when tt.typid = 1700 then 10 end as numeric_precision_radix,
        case when tt.typid in (21, 23, 20) then 0
             when tt.typid = 1700 then case when tt.typmod = -1 then null else (tt.typmod - 4) & 65535 end end as numeric_scale,
        case when tt.typid = 1082 then 0
             when tt.typid in (1083, 1114, 1184, 1266) then case when tt.typmod < 0 then 6 else tt.typmod end
             when tt.typid = 1186 then case when tt.typmod < 0 or tt.typmod & 65535 = 65535 then 6 else tt.typmod & 65535 end end as datetime_precision,
        case when t.typtype = 'd' then nt.nspname end as domain_schema,
        case when t.typtype = 'd' then t.typname end as domain_name,
        coalesce(nbt.nspname, nt.nspname) as udt_schema,
        coalesce(bt.typname, t.typname) as udt_name,
        coalesce(bt.typtype, t.typtype)::text as type_kind,
        elem.element_type_schema,
        elem.element_type_name,
        elem.element_type_kind,
        elem.labels
    from pg_catalog.pg_type t
        inner join pg_catalog.pg_namespace nt on nt.oid = t.typnamespace
        left join (pg_catalog.pg_type bt inner join pg_catalog.pg_namespace nbt on nbt.oid = bt.typnamespace)
            on t.typtype = 'd' and t.typbasetype = bt.oid
        cross join lateral (
            select
                case when t.typtype = 'd' then t.typbasetype else a.atttypid end as typid,
                case when t.typtype = 'd' then t.typtypmod else a.atttypmod end as typmod
        ) tt
        left join lateral (
            select
                elem_ns.nspname as element_type_schema,
                el.typname as element_type_name,
                el.typtype::text as element_type_kind,
                (
                    select array_agg(en.enumlabel::text order by en.enumsortorder)
                    from pg_catalog.pg_enum en
                    where en.enumtypid = case
                        when coalesce(bt.typtype, t.typtype) = 'e' then coalesce(bt.oid, t.oid)
                        when el.typtype = 'e' then el.oid end
                ) as labels
            from (select 1) dummy
                left join pg_catalog.pg_type el on el.oid = coalesce(bt.typelem, t.typelem)
                left join pg_catalog.pg_namespace elem_ns on elem_ns.oid = el.typnamespace
            offset 0
        ) elem on true
    where t.oid = a.atttypid
    offset 0
) ty
left join lateral (
    select nco.nspname, co.collname
    from pg_catalog.pg_collation co
        inner join pg_catalog.pg_namespace nco on nco.oid = co.collnamespace
    where co.oid = a.attcollation and (nco.nspname, co.collname) <> ('pg_catalog', 'default')
    offset 0
) coll on true
""";

    /// <summary>
    /// The <c>where</c> predicates, without the <c>where</c> keyword, that keep the live user columns
    /// the current user is allowed to see. The enclosing query's relation subquery leaves out
    /// relations in other sessions' temporary schemas.
    /// </summary>
    internal const string VisibleColumnsPredicate = """
a.attnum > 0 and not a.attisdropped
    and (pg_catalog.pg_has_role(c.relowner, 'USAGE')
        or pg_catalog.has_column_privilege(c.oid, a.attnum, 'SELECT, INSERT, UPDATE, REFERENCES'))
""";
}
