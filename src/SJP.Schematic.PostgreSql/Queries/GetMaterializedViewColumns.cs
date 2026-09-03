using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetMaterializedViewColumns
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// Name of the column
        /// </summary>
        public string? ColumnName { get; init; }

        /// <summary>
        /// Ordinal position of the column within the table (count starts at 1)
        /// </summary>
        public int OrdinalPosition { get; init; }

        /// <summary>
        /// Default expression of the column
        /// </summary>
        public string? ColumnDefault { get; init; }

        /// <summary>
        /// <c>YES</c> if the column is possibly nullable, <c>NO</c> if it is known not nullable. A not-null constraint is one way a column can be known not nullable, but there can be others.
        /// </summary>
        public string? IsNullable { get; init; }

        /// <summary>
        /// Data type of the column, if it is a built-in type, or <c>ARRAY</c> if it is some array (in that case, see the view element_types), else <c>USER-DEFINED</c> (in that case, the type is identified in <see cref="UdtName"/> and associated columns). If the column is based on a domain, this column refers to the type underlying the domain (and the domain is identified in <see cref="DomainName"/> and associated columns).
        /// </summary>
        public string? DataType { get; init; }

        /// <summary>
        /// If <see cref="DataType"/> identifies a character or bit string? type, the declared maximum length; null for all other data types or if no maximum length was declared.
        /// </summary>
        public int CharacterMaximumLength { get; init; }

        /// <summary>
        /// If <see cref="DataType"/> identifies a character type, the maximum possible length in octets (bytes) of a datum; null for all other data types. The maximum octet length depends on the declared character maximum length (see above) and the server encoding.
        /// </summary>
        public int CharacterOctetLength { get; init; }

        /// <summary>
        /// If <see cref="DataType"/> identifies a numeric type, this column contains the (declared or implicit) precision of the type for this column. The precision indicates the number of significant digits. It can be expressed in decimal (base 10) or binary (base 2) terms, as specified in the column <see cref="NumericPrecisionRadix"/>. For all other data types, this column is null.
        /// </summary>
        public int NumericPrecision { get; init; }

        /// <summary>
        /// If <see cref="DataType"/> identifies a numeric type, this column indicates in which base the values in the columns <see cref="NumericPrecision"/> and <see cref="NumericScale"/> are expressed. The value is either 2 or 10. For all other data types, this column is null.
        /// </summary>
        public int NumericPrecisionRadix { get; init; }

        /// <summary>
        /// If <see cref="DataType"/> identifies an exact numeric type, this column contains the (declared or implicit) scale of the type for this column. The scale indicates the number of significant digits to the right of the decimal point. It can be expressed in decimal (base 10) or binary (base 2) terms, as specified in the column <see cref="NumericPrecisionRadix"/>. For all other data types, this column is null.
        /// </summary>
        public int NumericScale { get; init; }

        /// <summary>
        /// If <see cref="DataType"/> identifies a date, time, timestamp, or interval type, this column contains the (declared or implicit) fractional seconds precision of the type for this column, that is, the number of decimal digits maintained following the decimal point in the seconds value. For all other data types, this column is null.
        /// </summary>
        public int? DatetimePrecision { get; init; }

        /// <summary>
        /// If <see cref="DataType"/> identifies an interval type, this column contains the specification which fields the intervals include for this column, e.g., <c>YEAR TO MONTH</c>, <c>DAY TO SECOND</c>, etc. If no field restrictions were specified (that is, the interval accepts all fields), and for all other data types, this field is null.
        /// </summary>
        public string? IntervalType { get; init; }

        /// <summary>
        /// Name of the database containing the collation of the column (always the current database), null if default or the data type of the column is not collatable
        /// </summary>
        public string? CollationCatalog { get; init; }

        /// <summary>
        /// Name of the schema containing the collation of the column, null if default or the data type of the column is not collatable
        /// </summary>
        public string? CollationSchema { get; init; }

        /// <summary>
        /// Name of the collation of the column, null if default or the data type of the column is not collatable
        /// </summary>
        public string? CollationName { get; init; }

        /// <summary>
        /// If the column has a domain type, the name of the database that the domain is defined in (always the current database), else null.
        /// </summary>
        public string? DomainCatalog { get; init; }

        /// <summary>
        /// If the column has a domain type, the name of the schema that the domain is defined in, else null.
        /// </summary>
        public string? DomainSchema { get; init; }

        /// <summary>
        /// If the column has a domain type, the name of the domain, else null.
        /// </summary>
        public string? DomainName { get; init; }

        /// <summary>
        /// Name of the database that the column data type (the underlying type of the domain, if applicable) is defined in (always the current database)
        /// </summary>
        public string? UdtCatalog { get; init; }

        /// <summary>
        /// Name of the schema that the column data type (the underlying type of the domain, if applicable) is defined in
        /// </summary>
        public string? UdtSchema { get; init; }

        /// <summary>
        /// Name of the column data type (the underlying type of the domain, if applicable)
        /// </summary>
        public string? UdtName { get; init; }

        /// <summary>
        /// An identifier of the data type descriptor of the column, unique among the data type descriptors pertaining to the table. This is mainly useful for joining with other instances of such identifiers. (The specific format of the identifier is not defined and not guaranteed to remain the same in future versions.)
        /// </summary>
        public string? DtdIdentifier { get; init; }

        /// <summary>
        /// A schema name for a sequence used to generate values. The column must be created from a serial keyword, otherwise the result will be <see langword="null" />.
        /// </summary>
        public string? SerialSequenceSchemaName { get; init; }

        /// <summary>
        /// A local name for a sequence used to generate values. This column be created from a serial keyword, otherwise the result will be <see langword="null" />.
        /// </summary>
        public string? SerialSequenceLocalName { get; init; }

        /// <summary>
        /// The <c>pg_type.typtype</c> of the column's type, e.g. <c>e</c> for an enum or <c>c</c> for a composite type.
        /// </summary>
        public string? TypeKind { get; init; }

        /// <summary>
        /// If the column's type is an array, the schema of its element type, else <see langword="null" />.
        /// </summary>
        public string? ElementTypeSchema { get; init; }

        /// <summary>
        /// If the column's type is an array, the name of its element type, else <see langword="null" />.
        /// </summary>
        public string? ElementTypeName { get; init; }

        /// <summary>
        /// If the column's type is an array, the <c>pg_type.typtype</c> of its element type, else <see langword="null" />.
        /// </summary>
        public string? ElementTypeKind { get; init; }

        /// <summary>
        /// The labels of whichever of the column's type or its element type is an enum, else <see langword="null" />.
        /// </summary>
        public string[]? EnumLabels { get; init; }
    }

    // taken largely from information_schema.sql for postgres (but modified to work with
    // matviews). The precision/scale/length computations that information_schema.columns
    // performs inline are delegated to the same information_schema._pg_* helper functions
    // that view uses internally - they ship as part of every postgresql installation.
    internal const string Sql = $"""

select
    a.attname as "{nameof(Result.ColumnName)}",
    a.attnum as "{nameof(Result.OrdinalPosition)}",
    pg_catalog.pg_get_expr(ad.adbin, ad.adrelid) as "{nameof(Result.ColumnDefault)}",
    case when a.attnotnull or (t.typtype = 'd' and t.typnotnull) then 'NO' else 'YES' end as "{nameof(Result.IsNullable)}",

    case when t.typtype = 'd' then
        case when bt.typelem <> 0 and bt.typlen = -1 then 'ARRAY'
             when nbt.nspname = 'pg_catalog' then pg_catalog.format_type(t.typbasetype, null)
             else 'USER-DEFINED' end
    else
        case when t.typelem <> 0 and t.typlen = -1 then 'ARRAY'
             when nt.nspname = 'pg_catalog' then pg_catalog.format_type(a.atttypid, null)
             else 'USER-DEFINED' end
    end as "{nameof(Result.DataType)}",

    information_schema._pg_char_max_length(
        information_schema._pg_truetypid(a, t), information_schema._pg_truetypmod(a, t)
    ) as "{nameof(Result.CharacterMaximumLength)}",
    information_schema._pg_char_octet_length(
        information_schema._pg_truetypid(a, t), information_schema._pg_truetypmod(a, t)
    ) as "{nameof(Result.CharacterOctetLength)}",
    information_schema._pg_numeric_precision(
        information_schema._pg_truetypid(a, t), information_schema._pg_truetypmod(a, t)
    ) as "{nameof(Result.NumericPrecision)}",
    information_schema._pg_numeric_precision_radix(
        information_schema._pg_truetypid(a, t), information_schema._pg_truetypmod(a, t)
    ) as "{nameof(Result.NumericPrecisionRadix)}",
    information_schema._pg_numeric_scale(
        information_schema._pg_truetypid(a, t), information_schema._pg_truetypmod(a, t)
    ) as "{nameof(Result.NumericScale)}",
    information_schema._pg_datetime_precision(
        information_schema._pg_truetypid(a, t), information_schema._pg_truetypmod(a, t)
    ) as "{nameof(Result.DatetimePrecision)}",
    information_schema._pg_interval_type(
        information_schema._pg_truetypid(a, t), information_schema._pg_truetypmod(a, t)
    ) as "{nameof(Result.IntervalType)}",

    case when nco.nspname is not null then pg_catalog.current_database() end as "{nameof(Result.CollationCatalog)}",
    nco.nspname as "{nameof(Result.CollationSchema)}",
    co.collname as "{nameof(Result.CollationName)}",

    case when t.typtype = 'd' then pg_catalog.current_database() else null end as "{nameof(Result.DomainCatalog)}",
    case when t.typtype = 'd' then nt.nspname else null end as "{nameof(Result.DomainSchema)}",
    case when t.typtype = 'd' then t.typname else null end as "{nameof(Result.DomainName)}",

    pg_catalog.current_database() as "{nameof(Result.UdtCatalog)}",
    coalesce(nbt.nspname, nt.nspname) as "{nameof(Result.UdtSchema)}",
    coalesce(bt.typname, t.typname) as "{nameof(Result.UdtName)}",

    a.attnum as "{nameof(Result.DtdIdentifier)}",

    coalesce(bt.typtype, t.typtype)::text as "{nameof(Result.TypeKind)}",
    elem_ns.nspname as "{nameof(Result.ElementTypeSchema)}",
    elem.typname as "{nameof(Result.ElementTypeName)}",
    elem.typtype::text as "{nameof(Result.ElementTypeKind)}",
    lbl.labels as "{nameof(Result.EnumLabels)}"

from (pg_catalog.pg_attribute a left join pg_catalog.pg_attrdef ad on attrelid = adrelid and attnum = adnum)
    join (pg_catalog.pg_class c join pg_catalog.pg_namespace nc on (c.relnamespace = nc.oid)) on a.attrelid = c.oid
    join (pg_catalog.pg_type t join pg_catalog.pg_namespace nt on (t.typnamespace = nt.oid)) on a.atttypid = t.oid
    left join (pg_catalog.pg_type bt join pg_catalog.pg_namespace nbt on (bt.typnamespace = nbt.oid))
    on (t.typtype = 'd' and t.typbasetype = bt.oid)
    left join (pg_catalog.pg_collation co join pg_catalog.pg_namespace nco on (co.collnamespace = nco.oid))
    on a.attcollation = co.oid and (nco.nspname, co.collname) <> ('pg_catalog', 'default')
    -- the type name reported above says nothing about what kind of type it is, what an array holds,
    -- or which labels an enum permits, so the catalog is read for those directly
    left join pg_catalog.pg_type elem on elem.oid = coalesce(bt.typelem, t.typelem)
    left join pg_catalog.pg_namespace elem_ns on elem_ns.oid = elem.typnamespace
    left join lateral (
        select array_agg(en.enumlabel::text order by en.enumsortorder) as labels
        from pg_catalog.pg_enum en
        where en.enumtypid = case
            when coalesce(bt.typtype, t.typtype) = 'e' then coalesce(bt.oid, t.oid)
            when elem.typtype = 'e' then elem.oid end
    ) lbl on true

where (not pg_catalog.pg_is_other_temp_schema(nc.oid))
    and a.attnum > 0 and not a.attisdropped
    and c.relkind = 'm' -- m = matview
    and (pg_catalog.pg_has_role(c.relowner, 'USAGE')
        or pg_catalog.has_column_privilege(c.oid, a.attnum, 'SELECT, INSERT, UPDATE, REFERENCES'))
    and nc.nspname = @{nameof(Query.SchemaName)} and c.relname = @{nameof(Query.ViewName)}
order by a.attnum -- ordinal_position
""";
}