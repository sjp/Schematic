using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableColumns
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
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
        /// A schema name for the sequence used to generate values, whether the column was declared with a serial type or as an identity column. <see langword="null" /> when no sequence backs the column.
        /// </summary>
        public string? SequenceSchemaName { get; init; }

        /// <summary>
        /// A local name for the sequence used to generate values, whether the column was declared with a serial type or as an identity column. <see langword="null" /> when no sequence backs the column.
        /// </summary>
        public string? SequenceLocalName { get; init; }

        /// <summary>
        /// The start value of the backing sequence, else <see langword="null" />. Read for serial columns, whose start value <c>information_schema.columns</c> does not report.
        /// </summary>
        public long? SequenceStart { get; init; }

        /// <summary>
        /// The increment of the backing sequence, else <see langword="null" />.
        /// </summary>
        public long? SequenceIncrement { get; init; }

        /// <summary>
        /// The minimum value of the backing sequence, else <see langword="null" />.
        /// </summary>
        public long? SequenceMinValue { get; init; }

        /// <summary>
        /// The maximum value of the backing sequence, else <see langword="null" />.
        /// </summary>
        public long? SequenceMaxValue { get; init; }

        /// <summary>
        /// Whether the backing sequence cycles, else <see langword="null" />.
        /// </summary>
        public bool? SequenceCycle { get; init; }

        /// <summary>
        /// If the column is an identity column, then <c>YES</c>, else <c>NO</c>.
        /// </summary>
        public string? IsIdentity { get; init; }

        /// <summary>
        /// If the column is an identity column, then <c>ALWAYS</c> or <c>BY DEFAULT</c>, reflecting the definition of the column.
        /// </summary>
        public string? IdentityGeneration { get; init; }

        /// <summary>
        /// If the column is an identity column, then the start value of the internal sequence, else <see langword="null" />.
        /// </summary>
        public string? IdentityStart { get; init; }

        /// <summary>
        /// If the column is an identity column, then the increment of the internal sequence, else <see langword="null" />.
        /// </summary>
        public string? IdentityIncrement { get; init; }

        /// <summary>
        /// If the column is an identity column, then the maximum value of the internal sequence, else <see langword="null" />.
        /// </summary>
        public string? IdentityMaximum { get; init; }

        /// <summary>
        /// If the column is an identity column, then the minimum value of the internal sequence, else <see langword="null" />.
        /// </summary>
        public string? IdentityMinimum { get; init; }

        /// <summary>
        /// If the column is an identity column, then <c>YES</c> if the internal sequence cycles or <c>NO</c> if it does not; otherwise <see langword="null" />.
        /// </summary>
        public string? IdentityCycle { get; init; }

        /// <summary>
        /// If the column is a generated column, then <c>ALWAYS</c>, else <c>NEVER</c>.
        /// </summary>
        public string? IsGenerated { get; init; }

        /// <summary>
        /// If the column is a generated column, then the generation expression, else null.
        /// </summary>
        public string? GenerationExpression { get; init; }

        /// <summary>
        /// <c>s</c> when a generated column is stored, <c>v</c> when it is computed on read, otherwise an empty string. <c>information_schema</c> does not report this.
        /// </summary>
        public string? GenerationKind { get; init; }
    }

    // a little bit convoluted due to the quote_ident() being required.
    // when missing, case folding will occur (we should have guaranteed that this is already done)
    // additionally the default behaviour misses the schema which may be necessary
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
    seq.parts[1] as "{nameof(Result.SequenceSchemaName)}",
    seq.parts[2] as "{nameof(Result.SequenceLocalName)}",
    s.start_value as "{nameof(Result.SequenceStart)}",
    s.increment_by as "{nameof(Result.SequenceIncrement)}",
    s.min_value as "{nameof(Result.SequenceMinValue)}",
    s.max_value as "{nameof(Result.SequenceMaxValue)}",
    s.cycle as "{nameof(Result.SequenceCycle)}",
    c.is_identity as "{nameof(Result.IsIdentity)}",
    c.identity_generation as "{nameof(Result.IdentityGeneration)}",
    c.identity_start as "{nameof(Result.IdentityStart)}",
    c.identity_increment as "{nameof(Result.IdentityIncrement)}",
    c.identity_maximum as "{nameof(Result.IdentityMaximum)}",
    c.identity_minimum as "{nameof(Result.IdentityMinimum)}",
    c.identity_cycle as "{nameof(Result.IdentityCycle)}",
    c.is_generated as "{nameof(Result.IsGenerated)}",
    c.generation_expression as "{nameof(Result.GenerationExpression)}",
    att.attgenerated::text as "{nameof(Result.GenerationKind)}"
from information_schema.columns c
-- pg_get_serial_sequence() resolves a column's owning sequence through pg_depend, which covers both
-- the sequence a serial default reads from and the one created for an identity column, so a single
-- call names the sequence behind either kind of generated column.
cross join lateral (
    select pg_catalog.parse_ident(
        pg_catalog.pg_get_serial_sequence(
            pg_catalog.quote_ident(c.table_schema) || '.' || pg_catalog.quote_ident(c.table_name),
            c.column_name
        )
    ) as parts
) seq
-- information_schema.columns reports start/increment/bounds for identity columns only, so a serial
-- column's parameters have to come from the sequence itself.
left join pg_catalog.pg_sequences s
    on s.schemaname = seq.parts[1] and s.sequencename = seq.parts[2]
-- information_schema.columns reports that a column is generated, but not whether its value is
-- stored with the row or computed on every read, which only pg_attribute.attgenerated says.
left join pg_catalog.pg_namespace ns on ns.nspname = c.table_schema
left join pg_catalog.pg_class cls on cls.relnamespace = ns.oid and cls.relname = c.table_name
left join pg_catalog.pg_attribute att on att.attrelid = cls.oid and att.attname = c.column_name
where c.table_schema = @{nameof(Query.SchemaName)} and c.table_name = @{nameof(Query.TableName)}
order by c.ordinal_position
""";
}