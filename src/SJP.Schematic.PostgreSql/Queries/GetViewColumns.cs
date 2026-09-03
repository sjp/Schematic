using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetViewColumns
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

    internal const string Sql = $"""

select
    c.column_name as "{nameof(Result.ColumnName)}",
    c.ordinal_position as "{nameof(Result.OrdinalPosition)}",
    c.column_default as "{nameof(Result.ColumnDefault)}",
    c.is_nullable as "{nameof(Result.IsNullable)}",
    c.data_type as "{nameof(Result.DataType)}",
    c.character_maximum_length as "{nameof(Result.CharacterMaximumLength)}",
    c.character_octet_length as "{nameof(Result.CharacterOctetLength)}",
    c.numeric_precision as "{nameof(Result.NumericPrecision)}",
    c.numeric_precision_radix as "{nameof(Result.NumericPrecisionRadix)}",
    c.numeric_scale as "{nameof(Result.NumericScale)}",
    c.datetime_precision as "{nameof(Result.DatetimePrecision)}",
    c.interval_type as "{nameof(Result.IntervalType)}",
    c.collation_catalog as "{nameof(Result.CollationCatalog)}",
    c.collation_schema as "{nameof(Result.CollationSchema)}",
    c.collation_name as "{nameof(Result.CollationName)}",
    c.domain_catalog as "{nameof(Result.DomainCatalog)}",
    c.domain_schema as "{nameof(Result.DomainSchema)}",
    c.domain_name as "{nameof(Result.DomainName)}",
    c.udt_catalog as "{nameof(Result.UdtCatalog)}",
    c.udt_schema as "{nameof(Result.UdtSchema)}",
    c.udt_name as "{nameof(Result.UdtName)}",
    c.dtd_identifier as "{nameof(Result.DtdIdentifier)}",
    udt.typtype::text as "{nameof(Result.TypeKind)}",
    elem_ns.nspname as "{nameof(Result.ElementTypeSchema)}",
    elem.typname as "{nameof(Result.ElementTypeName)}",
    elem.typtype::text as "{nameof(Result.ElementTypeKind)}",
    lbl.labels as "{nameof(Result.EnumLabels)}"
from information_schema.columns c
-- information_schema names a column's type as ARRAY or USER-DEFINED whenever it is not built in,
-- so the type itself is resolved through udt_name to learn what kind of type it is, what an array
-- holds, and which labels an enum permits
left join pg_catalog.pg_namespace udt_ns on udt_ns.nspname = c.udt_schema
left join pg_catalog.pg_type udt on udt.typnamespace = udt_ns.oid and udt.typname = c.udt_name
left join pg_catalog.pg_type elem on elem.oid = udt.typelem
left join pg_catalog.pg_namespace elem_ns on elem_ns.oid = elem.typnamespace
left join lateral (
    select array_agg(en.enumlabel::text order by en.enumsortorder) as labels
    from pg_catalog.pg_enum en
    where en.enumtypid = case when udt.typtype = 'e' then udt.oid when elem.typtype = 'e' then elem.oid end
) lbl on true
where c.table_schema = @{nameof(Query.SchemaName)} and c.table_name = @{nameof(Query.ViewName)}
order by c.ordinal_position
""";
}