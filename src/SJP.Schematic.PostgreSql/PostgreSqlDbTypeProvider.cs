using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql
{
    /// <summary>
    /// A database column type provider for PostgreSQL.
    /// </summary>
    /// <seealso cref="IDbTypeProvider" />
    public class PostgreSqlDbTypeProvider : IDbTypeProvider
    {
        /// <summary>
        /// Creates a column data type based on provided metadata.
        /// </summary>
        /// <param name="typeMetadata">Column type metadata.</param>
        /// <returns>A column data type.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <c>null</c>.</exception>
        public IDbType CreateColumnType(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            if (typeMetadata.TypeName == null)
                typeMetadata.TypeName = GetDefaultTypeName(typeMetadata);
            if (typeMetadata.DataType == DataType.Unknown)
                typeMetadata.DataType = GetDataType(typeMetadata.TypeName);
            if (typeMetadata.ClrType == null)
                typeMetadata.ClrType = GetClrType(typeMetadata.TypeName);
            typeMetadata.IsFixedLength = GetIsFixedLength(typeMetadata.TypeName);

            var definition = GetFormattedTypeName(typeMetadata);
            return new ColumnDataType(
                typeMetadata.TypeName,
                typeMetadata.DataType,
                definition,
                typeMetadata.ClrType,
                typeMetadata.IsFixedLength,
                typeMetadata.MaxLength,
                typeMetadata.NumericPrecision,
                typeMetadata.Collation
            );
        }

        /// <summary>
        /// Gets the data type that most closely matches the provided data type.
        /// </summary>
        /// <param name="otherType">An data type to compare with.</param>
        /// <returns>The closest matching column data type.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="otherType"/> is <c>null</c>.</exception>
        public IDbType GetComparableColumnType(IDbType otherType)
        {
            if (otherType == null)
                throw new ArgumentNullException(nameof(otherType));

            var typeMetadata = new ColumnTypeMetadata
            {
                ClrType = null, // ignoring so we get the default type provided
                Collation = otherType.Collation,
                DataType = otherType.DataType,
                IsFixedLength = otherType.IsFixedLength,
                MaxLength = otherType.MaxLength,
                NumericPrecision = otherType.NumericPrecision,
                TypeName = null // ignoring so we get a default name generated
            };

            return CreateColumnType(typeMetadata);
        }

        /// <summary>
        /// Gets the length of the is fixed.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns><c>true</c> if the type has a fixed length, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <c>null</c>, empty or whitespace.</exception>
        protected static bool GetIsFixedLength(Identifier typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return FixedLengthTypes.Contains(typeName);
        }

        /// <summary>
        /// Gets the default name of the type.
        /// </summary>
        /// <param name="typeMetadata">The type metadata.</param>
        /// <returns>A type name for the given type metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a type is unknown or failed to be parsed.</exception>
        protected static Identifier GetDefaultTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            return typeMetadata.DataType switch
            {
                DataType.Boolean => new Identifier("pg_catalog", "bool"),
                DataType.BigInteger => new Identifier("pg_catalog", "int8"),
                DataType.Binary => typeMetadata.IsFixedLength
                    ? new Identifier("pg_catalog", "bit")
                    : new Identifier("pg_catalog", "varbit"),
                DataType.LargeBinary => new Identifier("pg_catalog", "bytea"),
                DataType.Date => new Identifier("pg_catalog", "date"),
                DataType.DateTime => new Identifier("pg_catalog", "timestamp"),
                DataType.Float => new Identifier("pg_catalog", "float8"),
                DataType.Integer => new Identifier("pg_catalog", "int4"),
                DataType.Interval => new Identifier("pg_catalog", "interval"),
                DataType.Numeric => new Identifier("pg_catalog", "numeric"),
                DataType.SmallInteger => new Identifier("pg_catalog", "int2"),
                DataType.Time => new Identifier("pg_catalog", "time"),
                DataType.String or DataType.Unicode => typeMetadata.IsFixedLength
                    ? new Identifier("pg_catalog", "char")
                    : new Identifier("pg_catalog", "varchar"),
                DataType.Text or DataType.UnicodeText => new Identifier("pg_catalog", "text"),
                DataType.Unknown => throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for an unknown data type."),
                _ => throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for data type: " + typeMetadata.DataType.ToString()),
            };
        }

        /// <summary>
        /// Gets the name of the formatted type.
        /// </summary>
        /// <param name="typeMetadata">The type metadata.</param>
        /// <returns>A string representing a type name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when a type name is missing.</exception>
        protected static string GetFormattedTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));
            if (typeMetadata.TypeName == null)
                throw new ArgumentException("The type name is missing. A formatted type name cannot be generated.", nameof(typeMetadata));

            var builder = StringBuilderCache.Acquire(typeMetadata.TypeName.LocalName.Length * 2);
            var typeName = typeMetadata.TypeName;
            if (string.Equals(typeName.Schema, "pg_catalog", StringComparison.OrdinalIgnoreCase))
                builder.Append(QuoteIdentifier(typeName.LocalName));
            else
                builder.Append(QuoteName(typeName));

            if (TypeNamesWithNoLengthAnnotation.Contains(typeName))
                return builder.GetStringAndRelease();

            builder.Append('(');

            var npWithPrecisionOrScale = typeMetadata.NumericPrecision.Filter(np => np.Precision > 0 || np.Scale > 0);
            if (npWithPrecisionOrScale.IsSome)
            {
                npWithPrecisionOrScale.IfSome(precision =>
                {
                    builder.Append(precision.Precision.ToString(CultureInfo.InvariantCulture));
                    if (precision.Scale > 0)
                    {
                        builder.Append(", ");
                        builder.Append(precision.Scale.ToString(CultureInfo.InvariantCulture));
                    }
                });
            }
            else if (typeMetadata.MaxLength > 0)
            {
                var maxLength = typeMetadata.DataType == DataType.Unicode || typeMetadata.DataType == DataType.UnicodeText
                    ? typeMetadata.MaxLength / 2
                    : typeMetadata.MaxLength;

                builder.Append(maxLength.ToString(CultureInfo.InvariantCulture));
            }

            builder.Append(')');

            return builder.GetStringAndRelease();
        }

        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>A general data type class.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <c>null</c>, empty or whitespace.</exception>
        protected static DataType GetDataType(Identifier typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return StringToDataTypeMap.ContainsKey(typeName)
                ? StringToDataTypeMap[typeName]
                : DataType.Unknown;
        }

        /// <summary>
        /// Gets the CLR type for the associated type name.
        /// </summary>
        /// <param name="typeName">A type name.</param>
        /// <returns>A CLR type for the associated database type.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <c>null</c>, empty or whitespace.</exception>
        protected static Type GetClrType(Identifier typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return StringToClrTypeMap.ContainsKey(typeName)
                ? StringToClrTypeMap[typeName]
                : typeof(object);
        }

        /// <summary>
        /// Quotes an identifier component.
        /// </summary>
        /// <param name="identifier">An identifier component.</param>
        /// <returns>A quoted identifier component.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <c>null</c>, empty or whitespace.</exception>
        protected static string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return $"\"{ identifier.Replace("\"", "\"\"", StringComparison.Ordinal) }\"";
        }

        /// <summary>
        /// Quotes a type name.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <returns>A quoted type name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
        protected static string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var pieces = new List<string>();

            if (name.Server != null)
                pieces.Add(QuoteIdentifier(name.Server));
            if (name.Database != null)
                pieces.Add(QuoteIdentifier(name.Database));
            if (name.Schema != null)
                pieces.Add(QuoteIdentifier(name.Schema));
            if (name.LocalName != null)
                pieces.Add(QuoteIdentifier(name.LocalName));

            return pieces.Join(".");
        }

        private static readonly IEnumerable<Identifier> FixedLengthTypes = new HashSet<Identifier>(IdentifierComparer.Ordinal)
        {
            new Identifier("pg_catalog", "bit"),
            new Identifier("pg_catalog", "char"),
        };

        private static readonly IEnumerable<Identifier> TypeNamesWithNoLengthAnnotation = new HashSet<Identifier>(IdentifierComparer.Ordinal)
        {
            new Identifier("pg_catalog", "bigint"),
            new Identifier("pg_catalog", "int8"),
            new Identifier("pg_catalog", "bigserial"),
            new Identifier("pg_catalog", "serial8"),
            new Identifier("pg_catalog", "boolean"),
            new Identifier("pg_catalog", "bool"),
            new Identifier("pg_catalog", "box"),
            new Identifier("pg_catalog", "bytea"),
            new Identifier("pg_catalog", "cidr"),
            new Identifier("pg_catalog", "circle"),
            new Identifier("pg_catalog", "date"),
            new Identifier("pg_catalog", "float8"),
            new Identifier("pg_catalog", "inet"),
            new Identifier("pg_catalog", "integer"),
            new Identifier("pg_catalog", "int"),
            new Identifier("pg_catalog", "int4"),
            new Identifier("pg_catalog", "json"),
            new Identifier("pg_catalog", "jsonb"),
            new Identifier("pg_catalog", "line"),
            new Identifier("pg_catalog", "lseg"),
            new Identifier("pg_catalog", "macaddr"),
            new Identifier("pg_catalog", "macaddr8"),
            new Identifier("pg_catalog", "money"),
            new Identifier("pg_catalog", "path"),
            new Identifier("pg_catalog", "pg_lsn"),
            new Identifier("pg_catalog", "point"),
            new Identifier("pg_catalog", "polygon"),
            new Identifier("pg_catalog", "real"),
            new Identifier("pg_catalog", "float4"),
            new Identifier("pg_catalog", "smallint"),
            new Identifier("pg_catalog", "int2"),
            new Identifier("pg_catalog", "smallserial"),
            new Identifier("pg_catalog", "serial2"),
            new Identifier("pg_catalog", "serial"),
            new Identifier("pg_catalog", "serial4"),
            new Identifier("pg_catalog", "text"),
            new Identifier("pg_catalog", "tsquery"),
            new Identifier("pg_catalog", "tsvector"),
            new Identifier("pg_catalog", "txid_snapshot"),
            new Identifier("pg_catalog", "uuid"),
            new Identifier("pg_catalog", "xml")
        };

        private static readonly IReadOnlyDictionary<Identifier, DataType> StringToDataTypeMap = new Dictionary<Identifier, DataType>(IdentifierComparer.Ordinal)
        {
            [new Identifier("pg_catalog", "bigint")] = DataType.BigInteger,
            [new Identifier("pg_catalog", "int8")] = DataType.BigInteger,
            [new Identifier("pg_catalog", "bigserial")] = DataType.BigInteger,
            [new Identifier("pg_catalog", "int8")] = DataType.BigInteger,
            [new Identifier("pg_catalog", "bit")] = DataType.Binary,
            [new Identifier("pg_catalog", "bit varying")] = DataType.Binary,
            [new Identifier("pg_catalog", "varbit")] = DataType.Binary,
            [new Identifier("pg_catalog", "boolean")] = DataType.Boolean,
            [new Identifier("pg_catalog", "bool")] = DataType.Boolean,
            [new Identifier("pg_catalog", "bytea")] = DataType.LargeBinary,
            [new Identifier("pg_catalog", "character")] = DataType.Unicode,
            [new Identifier("pg_catalog", "char")] = DataType.Unicode,
            [new Identifier("pg_catalog", "character varying")] = DataType.Unicode,
            [new Identifier("pg_catalog", "varchar")] = DataType.Unicode,
            [new Identifier("pg_catalog", "text")] = DataType.UnicodeText,
            [new Identifier("pg_catalog", "date")] = DataType.Date,
            [new Identifier("pg_catalog", "double precision")] = DataType.Float,
            [new Identifier("pg_catalog", "float8")] = DataType.Float,
            [new Identifier("pg_catalog", "integer")] = DataType.Integer,
            [new Identifier("pg_catalog", "int")] = DataType.Integer,
            [new Identifier("pg_catalog", "int4")] = DataType.Integer,
            [new Identifier("pg_catalog", "interval")] = DataType.Interval,
            [new Identifier("pg_catalog", "json")] = DataType.UnicodeText,
            [new Identifier("pg_catalog", "jsonb")] = DataType.UnicodeText,
            [new Identifier("pg_catalog", "numeric")] = DataType.Numeric,
            [new Identifier("pg_catalog", "decimal")] = DataType.Numeric,
            [new Identifier("pg_catalog", "real")] = DataType.Float,
            [new Identifier("pg_catalog", "float4")] = DataType.Float,
            [new Identifier("pg_catalog", "smallint")] = DataType.SmallInteger,
            [new Identifier("pg_catalog", "int2")] = DataType.SmallInteger,
            [new Identifier("pg_catalog", "smallserial")] = DataType.SmallInteger,
            [new Identifier("pg_catalog", "serial2")] = DataType.SmallInteger,
            [new Identifier("pg_catalog", "serial")] = DataType.Integer,
            [new Identifier("pg_catalog", "serial4")] = DataType.Integer,
            [new Identifier("pg_catalog", "text")] = DataType.UnicodeText,
            [new Identifier("pg_catalog", "time")] = DataType.Time,
            [new Identifier("pg_catalog", "timetz")] = DataType.Time,
            [new Identifier("pg_catalog", "timestamp")] = DataType.DateTime,
            [new Identifier("pg_catalog", "timestamptz")] = DataType.DateTime,
            [new Identifier("pg_catalog", "xml")] = DataType.UnicodeText,
            [new Identifier("pg_catalog", "box")] = DataType.Unknown,
            [new Identifier("pg_catalog", "cidr")] = DataType.Unknown,
            [new Identifier("pg_catalog", "circle")] = DataType.Unknown,
            [new Identifier("pg_catalog", "inet")] = DataType.Unknown,
            [new Identifier("pg_catalog", "line")] = DataType.Unknown,
            [new Identifier("pg_catalog", "lseg")] = DataType.Unknown,
            [new Identifier("pg_catalog", "macaddr")] = DataType.Unknown,
            [new Identifier("pg_catalog", "macaddr8")] = DataType.Unknown,
            [new Identifier("pg_catalog", "money")] = DataType.Unknown,
            [new Identifier("pg_catalog", "path")] = DataType.Unknown,
            [new Identifier("pg_catalog", "pg_lsn")] = DataType.Unknown,
            [new Identifier("pg_catalog", "point")] = DataType.Unknown,
            [new Identifier("pg_catalog", "polygon")] = DataType.Unknown,
            [new Identifier("pg_catalog", "tsquery")] = DataType.Unknown,
            [new Identifier("pg_catalog", "tsvector")] = DataType.Unknown,
            [new Identifier("pg_catalog", "txid_snapshot")] = DataType.Unknown,
            [new Identifier("pg_catalog", "uuid")] = DataType.Unknown
        };

        private static readonly IReadOnlyDictionary<Identifier, Type> StringToClrTypeMap = new Dictionary<Identifier, Type>(IdentifierComparer.Ordinal)
        {
            [new Identifier("pg_catalog", "bigint")] = typeof(long),
            [new Identifier("pg_catalog", "int8")] = typeof(long),
            [new Identifier("pg_catalog", "bigserial")] = typeof(long),
            [new Identifier("pg_catalog", "int8")] = typeof(long),
            [new Identifier("pg_catalog", "bit")] = typeof(byte[]),
            [new Identifier("pg_catalog", "bit varying")] = typeof(byte[]),
            [new Identifier("pg_catalog", "varbit")] = typeof(byte[]),
            [new Identifier("pg_catalog", "boolean")] = typeof(bool),
            [new Identifier("pg_catalog", "bool")] = typeof(bool),
            [new Identifier("pg_catalog", "bytea")] = typeof(byte[]),
            [new Identifier("pg_catalog", "character")] = typeof(string),
            [new Identifier("pg_catalog", "char")] = typeof(string),
            [new Identifier("pg_catalog", "character varying")] = typeof(string),
            [new Identifier("pg_catalog", "varchar")] = typeof(string),
            [new Identifier("pg_catalog", "text")] = typeof(string),
            [new Identifier("pg_catalog", "date")] = typeof(DateTime),
            [new Identifier("pg_catalog", "double precision")] = typeof(double),
            [new Identifier("pg_catalog", "float8")] = typeof(double),
            [new Identifier("pg_catalog", "integer")] = typeof(int),
            [new Identifier("pg_catalog", "int")] = typeof(int),
            [new Identifier("pg_catalog", "int4")] = typeof(int),
            [new Identifier("pg_catalog", "interval")] = typeof(TimeSpan),
            [new Identifier("pg_catalog", "json")] = typeof(string),
            [new Identifier("pg_catalog", "jsonb")] = typeof(string),
            [new Identifier("pg_catalog", "numeric")] = typeof(decimal),
            [new Identifier("pg_catalog", "decimal")] = typeof(decimal),
            [new Identifier("pg_catalog", "real")] = typeof(float),
            [new Identifier("pg_catalog", "float4")] = typeof(float),
            [new Identifier("pg_catalog", "smallint")] = typeof(short),
            [new Identifier("pg_catalog", "int2")] = typeof(short),
            [new Identifier("pg_catalog", "smallserial")] = typeof(short),
            [new Identifier("pg_catalog", "serial2")] = typeof(short),
            [new Identifier("pg_catalog", "serial")] = typeof(int),
            [new Identifier("pg_catalog", "serial4")] = typeof(int),
            [new Identifier("pg_catalog", "text")] = typeof(string),
            [new Identifier("pg_catalog", "time")] = typeof(TimeSpan),
            [new Identifier("pg_catalog", "timetz")] = typeof(TimeSpan),
            [new Identifier("pg_catalog", "timestamp")] = typeof(DateTime),
            [new Identifier("pg_catalog", "timestamptz")] = typeof(DateTime),
            [new Identifier("pg_catalog", "xml")] = typeof(string),
            [new Identifier("pg_catalog", "box")] = typeof(object),
            [new Identifier("pg_catalog", "cidr")] = typeof(object),
            [new Identifier("pg_catalog", "circle")] = typeof(object),
            [new Identifier("pg_catalog", "inet")] = typeof(object),
            [new Identifier("pg_catalog", "line")] = typeof(object),
            [new Identifier("pg_catalog", "lseg")] = typeof(object),
            [new Identifier("pg_catalog", "macaddr")] = typeof(object),
            [new Identifier("pg_catalog", "macaddr8")] = typeof(object),
            [new Identifier("pg_catalog", "money")] = typeof(object),
            [new Identifier("pg_catalog", "path")] = typeof(object),
            [new Identifier("pg_catalog", "pg_lsn")] = typeof(object),
            [new Identifier("pg_catalog", "point")] = typeof(object),
            [new Identifier("pg_catalog", "polygon")] = typeof(object),
            [new Identifier("pg_catalog", "tsquery")] = typeof(object),
            [new Identifier("pg_catalog", "tsvector")] = typeof(object),
            [new Identifier("pg_catalog", "txid_snapshot")] = typeof(object),
            [new Identifier("pg_catalog", "uuid")] = typeof(object)
        };
    }
}
