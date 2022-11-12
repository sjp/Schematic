namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetViewColumns
{
    internal sealed record Query
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
        public int DatetimePrecision { get; init; }

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
        ///	Name of the collation of the column, null if default or the data type of the column is not collatable
        /// </summary>
        public string? CollationName { get; init; }

        /// <summary>
        /// If the column has a domain type, the name of the database that the domain is defined in (always the current database), else null.
        /// </summary>
        public string? DomainCatalog { get; init; }

        /// <summary>
        ///	If the column has a domain type, the name of the schema that the domain is defined in, else null.
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
        /// A schema name for a sequence used to generate values. The column must be created from a serial keyword, otherwise the result will be <c>null</c>.
        /// </summary>
        public string? SerialSequenceSchemaName { get; init; }

        /// <summary>
        /// A local name for a sequence used to generate values. This column be created from a serial keyword, otherwise the result will be <c>null</c>.
        /// </summary>
        public string? SerialSequenceLocalName { get; init; }
    }

    internal const string Sql = @$"
select
    column_name as ""{nameof(Result.ColumnName)}"",
    ordinal_position as ""{nameof(Result.OrdinalPosition)}"",
    column_default as ""{nameof(Result.ColumnDefault)}"",
    is_nullable as ""{nameof(Result.IsNullable)}"",
    data_type as ""{nameof(Result.DataType)}"",
    character_maximum_length as ""{nameof(Result.CharacterMaximumLength)}"",
    character_octet_length as ""{nameof(Result.CharacterOctetLength)}"",
    numeric_precision as ""{nameof(Result.NumericPrecision)}"",
    numeric_precision_radix as ""{nameof(Result.NumericPrecisionRadix)}"",
    numeric_scale as ""{nameof(Result.NumericScale)}"",
    datetime_precision as ""{nameof(Result.DatetimePrecision)}"",
    interval_type as ""{nameof(Result.IntervalType)}"",
    collation_catalog as ""{nameof(Result.CollationCatalog)}"",
    collation_schema as ""{nameof(Result.CollationSchema)}"",
    collation_name as ""{nameof(Result.CollationName)}"",
    domain_catalog as ""{nameof(Result.DomainCatalog)}"",
    domain_schema as ""{nameof(Result.DomainSchema)}"",
    domain_name as ""{nameof(Result.DomainName)}"",
    udt_catalog as ""{nameof(Result.UdtCatalog)}"",
    udt_schema as ""{nameof(Result.UdtSchema)}"",
    udt_name as ""{nameof(Result.UdtName)}"",
    dtd_identifier as ""{nameof(Result.DtdIdentifier)}""
from information_schema.columns
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.ViewName)}
order by ordinal_position";
}