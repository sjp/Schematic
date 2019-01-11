namespace SJP.Schematic.PostgreSql.Query
{
    internal class ColumnData
    {
        /// <summary>
        /// Name of the column
        /// </summary>
        public string column_name { get; set; }

        /// <summary>
        /// Ordinal position of the column within the table (count starts at 1)
        /// </summary>
        public int ordinal_position { get; set; }

        /// <summary>
        /// Default expression of the column
        /// </summary>
        public string column_default { get; set; }

        /// <summary>
        /// <c>YES</c> if the column is possibly nullable, <c>NO</c> if it is known not nullable. A not-null constraint is one way a column can be known not nullable, but there can be others.
        /// </summary>
        public string is_nullable { get; set; }

        /// <summary>
        /// Data type of the column, if it is a built-in type, or <c>ARRAY</c> if it is some array (in that case, see the view element_types), else <c>USER-DEFINED</c> (in that case, the type is identified in <see cref="udt_name"/> and associated columns). If the column is based on a domain, this column refers to the type underlying the domain (and the domain is identified in <see cref="domain_name"/> and associated columns).
        /// </summary>
        public string data_type { get; set; }

        /// <summary>
        /// If <see cref="data_type"/> identifies a character or bit string type, the declared maximum length; null for all other data types or if no maximum length was declared.
        /// </summary>
        public int character_maximum_length { get; set; }

        /// <summary>
        /// If <see cref="data_type"/> identifies a character type, the maximum possible length in octets (bytes) of a datum; null for all other data types. The maximum octet length depends on the declared character maximum length (see above) and the server encoding.
        /// </summary>
        public int character_octet_length { get; set; }

        /// <summary>
        /// If <see cref="data_type"/> identifies a numeric type, this column contains the (declared or implicit) precision of the type for this column. The precision indicates the number of significant digits. It can be expressed in decimal (base 10) or binary (base 2) terms, as specified in the column <see cref="numeric_precision_radix"/>. For all other data types, this column is null.
        /// </summary>
        public int numeric_precision { get; set; }

        /// <summary>
        /// If <see cref="data_type"/> identifies a numeric type, this column indicates in which base the values in the columns <see cref="numeric_precision"/> and <see cref="numeric_scale"/> are expressed. The value is either 2 or 10. For all other data types, this column is null.
        /// </summary>
        public int numeric_precision_radix { get; set; }

        /// <summary>
        /// If <see cref="data_type"/> identifies an exact numeric type, this column contains the (declared or implicit) scale of the type for this column. The scale indicates the number of significant digits to the right of the decimal point. It can be expressed in decimal (base 10) or binary (base 2) terms, as specified in the column <see cref="numeric_precision_radix"/>. For all other data types, this column is null.
        /// </summary>
        public int numeric_scale { get; set; }

        /// <summary>
        /// If <see cref="data_type"/> identifies a date, time, timestamp, or interval type, this column contains the (declared or implicit) fractional seconds precision of the type for this column, that is, the number of decimal digits maintained following the decimal point in the seconds value. For all other data types, this column is null.
        /// </summary>
        public int datetime_precision { get; set; }

        /// <summary>
        /// If <see cref="data_type"/> identifies an interval type, this column contains the specification which fields the intervals include for this column, e.g., <c>YEAR TO MONTH</c>, <c>DAY TO SECOND</c>, etc. If no field restrictions were specified (that is, the interval accepts all fields), and for all other data types, this field is null.
        /// </summary>
        public string interval_type { get; set; }

        /// <summary>
        /// Name of the database containing the collation of the column (always the current database), null if default or the data type of the column is not collatable
        /// </summary>
        public string collation_catalog { get; set; }

        /// <summary>
        /// Name of the schema containing the collation of the column, null if default or the data type of the column is not collatable
        /// </summary>
        public string collation_schema { get; set; }

        /// <summary>
        ///	Name of the collation of the column, null if default or the data type of the column is not collatable
        /// </summary>
        public string collation_name { get; set; }

        /// <summary>
        /// If the column has a domain type, the name of the database that the domain is defined in (always the current database), else null.
        /// </summary>
        public string domain_catalog { get; set; }

        /// <summary>
        ///	If the column has a domain type, the name of the schema that the domain is defined in, else null.
        /// </summary>
        public string domain_schema { get; set; }

        /// <summary>
        /// If the column has a domain type, the name of the domain, else null.
        /// </summary>
        public string domain_name { get; set; }

        /// <summary>
        /// Name of the database that the column data type (the underlying type of the domain, if applicable) is defined in (always the current database)
        /// </summary>
        public string udt_catalog { get; set; }

        /// <summary>
        /// Name of the schema that the column data type (the underlying type of the domain, if applicable) is defined in
        /// </summary>
        public string udt_schema { get; set; }

        /// <summary>
        /// Name of the column data type (the underlying type of the domain, if applicable)
        /// </summary>
        public string udt_name { get; set; }

        /// <summary>
        /// An identifier of the data type descriptor of the column, unique among the data type descriptors pertaining to the table. This is mainly useful for joining with other instances of such identifiers. (The specific format of the identifier is not defined and not guaranteed to remain the same in future versions.)
        /// </summary>
        public string dtd_identifier { get; set; }

        /// <summary>
        /// A schema name for a sequence used to generate values. The column must be created from a serial keyword, otherwise the result will be <c>null</c>.
        /// </summary>
        public string serial_sequence_schema_name { get; set; }

        /// <summary>
        /// A local name for a sequence used to generate values. This column be created from a serial keyword, otherwise the result will be <c>null</c>.
        /// </summary>
        public string serial_sequence_local_name { get; set; }
    }
}
