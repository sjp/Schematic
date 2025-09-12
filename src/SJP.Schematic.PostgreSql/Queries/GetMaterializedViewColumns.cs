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
        /// A schema name for a sequence used to generate values. The column must be created from a serial keyword, otherwise the result will be <see langword="null" />.
        /// </summary>
        public string? SerialSequenceSchemaName { get; init; }

        /// <summary>
        /// A local name for a sequence used to generate values. This column be created from a serial keyword, otherwise the result will be <see langword="null" />.
        /// </summary>
        public string? SerialSequenceLocalName { get; init; }
    }

    // taken largely from information_schema.sql for postgres (but modified to work with matviews)
    internal const string Sql = $"""

SELECT
    a.attname AS "{nameof(Result.ColumnName)}",
    a.attnum AS "{nameof(Result.OrdinalPosition)}",
    pg_catalog.pg_get_expr(ad.adbin, ad.adrelid) AS "{nameof(Result.ColumnDefault)}",
    CASE WHEN a.attnotnull OR (t.typtype = 'd' AND t.typnotnull) THEN 'NO' ELSE 'YES' END
        AS "{nameof(Result.IsNullable)}",

    CASE WHEN t.typtype = 'd' THEN
    CASE WHEN bt.typelem <> 0 AND bt.typlen = -1 THEN 'ARRAY'
        WHEN nbt.nspname = 'pg_catalog' THEN format_type(t.typbasetype, null)
        ELSE 'USER-DEFINED' END
    ELSE
    CASE WHEN t.typelem <> 0 AND t.typlen = -1 THEN 'ARRAY'
        WHEN nt.nspname = 'pg_catalog' THEN format_type(a.atttypid, null)
        ELSE 'USER-DEFINED' END
    END
    AS "{nameof(Result.DataType)}",


""" + PgCharMaxLength + $"""

    AS "{nameof(Result.CharacterMaximumLength)}",


""" + PgCharOctetLength + $"""

    AS "{nameof(Result.CharacterOctetLength)}",


""" + PgNumericPrecision + $"""

    AS "{nameof(Result.NumericPrecision)}",


""" + PgNumericPrecisionRadix + $"""

    AS "{nameof(Result.NumericPrecisionRadix)}",


""" + PgNumericScale + $"""

    AS "{nameof(Result.NumericScale)}",


""" + PgDatetimePrecision + $"""

    AS "{nameof(Result.DatetimePrecision)}",


""" + PgIntervalType + $"""

    AS "{nameof(Result.IntervalType)}",

    CASE WHEN nco.nspname IS NOT NULL THEN current_database() END AS "{nameof(Result.CollationCatalog)}",
    nco.nspname AS "{nameof(Result.CollationSchema)}",
    co.collname AS "{nameof(Result.CollationName)}",

    CASE WHEN t.typtype = 'd' THEN current_database() ELSE null END
        AS "{nameof(Result.DomainCatalog)}",
    CASE WHEN t.typtype = 'd' THEN nt.nspname ELSE null END
        AS "{nameof(Result.DomainSchema)}",
    CASE WHEN t.typtype = 'd' THEN t.typname ELSE null END
        AS "{nameof(Result.DomainName)}",

    current_database() AS "{nameof(Result.UdtCatalog)}",
    coalesce(nbt.nspname, nt.nspname) AS "{nameof(Result.UdtSchema)}",
    coalesce(bt.typname, t.typname) AS "{nameof(Result.UdtName)}",

    a.attnum AS "{nameof(Result.DtdIdentifier)}"

FROM (pg_catalog.pg_attribute a LEFT JOIN pg_catalog.pg_attrdef ad ON attrelid = adrelid AND attnum = adnum)
    JOIN (pg_catalog.pg_class c JOIN pg_catalog.pg_namespace nc ON (c.relnamespace = nc.oid)) ON a.attrelid = c.oid
    JOIN (pg_catalog.pg_type t JOIN pg_catalog.pg_namespace nt ON (t.typnamespace = nt.oid)) ON a.atttypid = t.oid
    LEFT JOIN (pg_catalog.pg_type bt JOIN pg_catalog.pg_namespace nbt ON (bt.typnamespace = nbt.oid))
    ON (t.typtype = 'd' AND t.typbasetype = bt.oid)
    LEFT JOIN (pg_catalog.pg_collation co JOIN pg_catalog.pg_namespace nco ON (co.collnamespace = nco.oid))
    ON a.attcollation = co.oid AND (nco.nspname, co.collname) <> ('pg_catalog', 'default')

WHERE (NOT pg_catalog.pg_is_other_temp_schema(nc.oid))

        AND a.attnum > 0 AND NOT a.attisdropped
        AND c.relkind = 'm' -- m = matview

        AND (pg_catalog.pg_has_role(c.relowner, 'USAGE')
            OR has_column_privilege(c.oid, a.attnum,
                                    'SELECT, INSERT, UPDATE, REFERENCES'))
        AND nc.nspname = @{nameof(Query.SchemaName)} and c.relname = @{nameof(Query.ViewName)}
ORDER BY a.attnum -- ordinal_position
""";

    // In order to cleanly build up an equivalent of information_schema.columns view
    // we also need to have some functions available. However, because we do not want
    // to modify any existing schema, we will build up this query via string replacement.
    // Normally this is unsafe, but we will have *no user input* for the query, and any
    // parameters are already parameterised

    private const string PgTrueTypId = "CASE WHEN t.typtype = 'd' THEN t.typbasetype ELSE a.atttypid END";
    private const string PgTrueTypMod = "CASE WHEN t.typtype = 'd' THEN t.typtypmod ELSE a.atttypmod END";

    private const string PgCharMaxLength = "CASE WHEN " + PgTrueTypMod + @" = -1 /* default typmod */
       THEN null
       WHEN " + PgTrueTypId + @" IN (1042, 1043) /* char, varchar */
       THEN " + PgTrueTypMod + @" - 4
       WHEN " + PgTrueTypId + @" IN (1560, 1562) /* bit, varbit */
       THEN " + PgTrueTypMod + @"
       ELSE null
  END";

    private const string PgCharOctetLength = "CASE WHEN " + PgTrueTypId + @" IN (25, 1042, 1043) /* text, char, varchar */
       THEN CASE WHEN " + PgTrueTypMod + @" = -1 /* default typmod */
                 THEN CAST(2^30 AS integer)
                 ELSE " + PgCharMaxLength + @" *
                      pg_catalog.pg_encoding_max_length((SELECT encoding FROM pg_catalog.pg_database WHERE datname = pg_catalog.current_database()))
            END
       ELSE null
  END";

    private const string PgNumericPrecision = "CASE " + PgTrueTypId + @"
         WHEN 21 /*int2*/ THEN 16
         WHEN 23 /*int4*/ THEN 32
         WHEN 20 /*int8*/ THEN 64
         WHEN 1700 /*numeric*/ THEN
              CASE WHEN " + PgTrueTypMod + @" = -1
                   THEN null
                   ELSE ((" + PgTrueTypMod + @" - 4) >> 16) & 65535
                   END
         WHEN 700 /*float4*/ THEN 24 /*FLT_MANT_DIG*/
         WHEN 701 /*float8*/ THEN 53 /*DBL_MANT_DIG*/
         ELSE null
  END";

    private const string PgNumericPrecisionRadix = "CASE WHEN " + PgTrueTypId + @" IN (21, 23, 20, 700, 701) THEN 2
       WHEN " + PgTrueTypId + @" IN (1700) THEN 10
       ELSE null
  END";

    private const string PgNumericScale = "CASE WHEN " + PgTrueTypId + @" IN (21, 23, 20) THEN 0
       WHEN " + PgTrueTypId + @" IN (1700) THEN
            CASE WHEN " + PgTrueTypMod + @" = -1
                 THEN null
                 ELSE (" + PgTrueTypMod + @" - 4) & 65535
                 END
       ELSE null
  END";

    private const string PgDatetimePrecision = "CASE WHEN " + PgTrueTypId + @" IN (1082) /* date */
           THEN 0
       WHEN " + PgTrueTypId + @" IN (1083, 1114, 1184, 1266) /* time, timestamp, same + tz */
           THEN CASE WHEN " + PgTrueTypMod + " < 0 THEN 6 ELSE " + PgTrueTypMod + @" END
       WHEN " + PgTrueTypId + @" IN (1186) /* interval */
           THEN CASE WHEN " + PgTrueTypMod + " < 0 OR " + PgTrueTypMod + " & 65535 = 65535 THEN 6 ELSE " + PgTrueTypMod + @" & 65535 END
       ELSE null
  END";

    private const string PgIntervalType = "CASE WHEN " + PgTrueTypId + @" IN (1186) /* interval */
           THEN pg_catalog.upper(substring(pg_catalog.format_type(" + PgTrueTypId + ", " + PgTrueTypMod + """
) from 'interval[()0-9]* #" %#"' for '#'))
       ELSE null
  END
""";
}