using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableColumns
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result : IColumnCatalogRow
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
        /// If the column has a domain type, the name of the schema that the domain is defined in, else null.
        /// </summary>
        public string? DomainSchema { get; init; }

        /// <summary>
        /// If the column has a domain type, the name of the domain, else null.
        /// </summary>
        public string? DomainName { get; init; }

        /// <summary>
        /// Name of the schema that the column data type (the underlying type of the domain, if applicable) is defined in
        /// </summary>
        public string? UdtSchema { get; init; }

        /// <summary>
        /// Name of the column data type (the underlying type of the domain, if applicable)
        /// </summary>
        public string? UdtName { get; init; }

        /// <summary>
        /// A schema name for the sequence used to generate values, whether the column was declared with a serial type or as an identity column. <see langword="null" /> when no sequence backs the column.
        /// </summary>
        public string? SequenceSchemaName { get; init; }

        /// <summary>
        /// A local name for the sequence used to generate values, whether the column was declared with a serial type or as an identity column. <see langword="null" /> when no sequence backs the column.
        /// </summary>
        public string? SequenceLocalName { get; init; }

        /// <summary>
        /// The start value of the backing sequence, else <see langword="null" />.
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
        /// The <c>pg_attribute.attidentity</c> of the column: <c>a</c> for an identity column generated always, <c>d</c> for one generated by default, otherwise an empty string.
        /// </summary>
        public string? IdentityKind { get; init; }

        /// <summary>
        /// If the column is a generated column, then the generation expression, else null.
        /// </summary>
        public string? GenerationExpression { get; init; }

        /// <summary>
        /// The <c>pg_attribute.attgenerated</c> of the column: <c>s</c> when a generated column is stored, <c>v</c> when it is computed on read, otherwise an empty string.
        /// </summary>
        public string? GenerationKind { get; init; }

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
{ColumnCatalogSql.SelectList},
    seq.schema_name as "{nameof(Result.SequenceSchemaName)}",
    seq.sequence_name as "{nameof(Result.SequenceLocalName)}",
    seq.seqstart as "{nameof(Result.SequenceStart)}",
    seq.seqincrement as "{nameof(Result.SequenceIncrement)}",
    seq.seqmin as "{nameof(Result.SequenceMinValue)}",
    seq.seqmax as "{nameof(Result.SequenceMaxValue)}",
    seq.seqcycle as "{nameof(Result.SequenceCycle)}",
    a.attidentity::text as "{nameof(Result.IdentityKind)}",
    case when a.attgenerated <> '' then pg_catalog.pg_get_expr(ad.adbin, ad.adrelid) end as "{nameof(Result.GenerationExpression)}",
    a.attgenerated::text as "{nameof(Result.GenerationKind)}"
{ColumnCatalogSql.From}
    -- The sequence behind a column is the one that depends on the column itself: automatically for a
    -- sequence owned by a serial column, internally for the one created for an identity column. This
    -- is the lookup pg_get_serial_sequence() performs, done by OID rather than by a name it would
    -- have to parse. Should a column own more than one sequence, the identity sequence and then the
    -- oldest sequence are preferred.
    left join lateral (
        select seq_ns.nspname as schema_name, seq_cls.relname as sequence_name,
            ps.seqstart, ps.seqincrement, ps.seqmin, ps.seqmax, ps.seqcycle
        from pg_catalog.pg_depend dep
            inner join pg_catalog.pg_class seq_cls on seq_cls.oid = dep.objid and seq_cls.relkind = 'S'
            inner join pg_catalog.pg_namespace seq_ns on seq_ns.oid = seq_cls.relnamespace
            inner join pg_catalog.pg_sequence ps on ps.seqrelid = seq_cls.oid
        where dep.refclassid = 'pg_catalog.pg_class'::regclass
            and dep.refobjid = a.attrelid
            and dep.refobjsubid = a.attnum
            and dep.classid = 'pg_catalog.pg_class'::regclass
            and dep.deptype in ('a', 'i')
        order by dep.deptype = 'i' desc, dep.objid
        limit 1
    ) seq on true
where {ColumnCatalogSql.VisibleColumnsPredicate}
    and c.relkind in ('r', 'p')
    and nc.nspname = @{nameof(Query.SchemaName)} and c.relname = @{nameof(Query.TableName)}
order by a.attnum
""";
}