using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDbTypeProvider : IDbTypeProvider
    {
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

        protected bool GetIsFixedLength(Identifier typeName)
        {
            if (typeName == null || typeName.LocalName == null)
                throw new ArgumentNullException(nameof(typeName));

            return _fixedLengthTypes.Contains(typeName);
        }

        protected Identifier GetDefaultTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            switch (typeMetadata.DataType)
            {
                case DataType.Boolean:
                    return new Identifier("pg_catalog", "bool");
                case DataType.BigInteger:
                    return new Identifier("pg_catalog", "int8");
                case DataType.Binary:
                    return typeMetadata.IsFixedLength
                        ? new Identifier("pg_catalog", "bit")
                        : new Identifier("pg_catalog", "varbit");
                case DataType.LargeBinary:
                    return new Identifier("pg_catalog", "bytea");
                case DataType.Date:
                    return new Identifier("pg_catalog", "date");
                case DataType.DateTime:
                    return new Identifier("pg_catalog", "timestamp");
                case DataType.Float:
                    return new Identifier("pg_catalog", "float8");
                case DataType.Integer:
                    return new Identifier("pg_catalog", "int4");
                case DataType.Interval:
                    return new Identifier("pg_catalog", "interval");
                case DataType.Numeric:
                    return new Identifier("pg_catalog", "numeric");
                case DataType.SmallInteger:
                    return new Identifier("pg_catalog", "int2");
                case DataType.Time:
                    return new Identifier("pg_catalog", "time");
                case DataType.String:
                case DataType.Unicode:
                    return typeMetadata.IsFixedLength
                        ? new Identifier("pg_catalog", "char")
                        : new Identifier("pg_catalog", "varchar");
                case DataType.Text:
                case DataType.UnicodeText:
                    return new Identifier("pg_catalog", "text");
                case DataType.Unknown:
                    throw new ArgumentOutOfRangeException("Unable to determine a type name for an unknown data type.", nameof(typeMetadata));
                default:
                    throw new ArgumentOutOfRangeException("Unable to determine a type name for data type: " + typeMetadata.DataType.ToString(), nameof(typeMetadata));
            }
        }

        protected string GetFormattedTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));
            if (typeMetadata.TypeName == null)
                throw new ArgumentException("The type name is missing. A formatted type name cannot be generated.", nameof(typeMetadata));

            var builder = new StringBuilder(typeMetadata.TypeName.LocalName.Length * 2);
            var typeName = typeMetadata.TypeName;
            if (string.Equals(typeName.Schema, "pg_catalog", StringComparison.OrdinalIgnoreCase))
                builder.Append(QuoteIdentifier(typeName.LocalName));
            else
                builder.Append(QuoteName(typeName));

            if (_typeNamesWithNoLengthAnnotation.Contains(typeName))
                return builder.ToString();

            builder.Append("(");

            if (typeMetadata.NumericPrecision.Precision > 0)
            {
                builder.Append(typeMetadata.NumericPrecision.Precision.ToString(CultureInfo.InvariantCulture));
                if (typeMetadata.NumericPrecision.Scale > 0)
                {
                    builder.Append(", ");
                    builder.Append(typeMetadata.NumericPrecision.Scale.ToString(CultureInfo.InvariantCulture));
                }
            }
            else if (typeMetadata.MaxLength > 0)
            {
                var maxLength = typeMetadata.DataType == DataType.Unicode || typeMetadata.DataType == DataType.UnicodeText
                    ? typeMetadata.MaxLength / 2
                    : typeMetadata.MaxLength;

                builder.Append(maxLength.ToString(CultureInfo.InvariantCulture));
            }

            builder.Append(")");

            return builder.ToString();
        }

        protected DataType GetDataType(Identifier typeName)
        {
            if (typeName == null || typeName.LocalName == null)
                throw new ArgumentNullException(nameof(typeName));

            return _stringToDataTypeMap.ContainsKey(typeName)
                ? _stringToDataTypeMap[typeName]
                : DataType.Unknown;
        }

        protected Type GetClrType(Identifier typeName)
        {
            if (typeName == null || typeName.LocalName == null)
                throw new ArgumentNullException(nameof(typeName));

            return _stringToClrTypeMap.ContainsKey(typeName)
                ? _stringToClrTypeMap[typeName]
                : typeof(object);
        }

        protected static string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return $"\"{ identifier.Replace("\"", "\"\"") }\"";
        }

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

        private readonly static IEnumerable<Identifier> _fixedLengthTypes = new HashSet<Identifier>(IdentifierComparer.Ordinal)
        {
            new Identifier("pg_catalog", "bit"),
            new Identifier("pg_catalog", "char"),
        };

        private readonly static IEnumerable<Identifier> _typeNamesWithNoLengthAnnotation = new HashSet<Identifier>(IdentifierComparer.Ordinal)
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

        private readonly static IReadOnlyDictionary<Identifier, DataType> _stringToDataTypeMap = new Dictionary<Identifier, DataType>(IdentifierComparer.Ordinal)
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

        private readonly static IReadOnlyDictionary<Identifier, Type> _stringToClrTypeMap = new Dictionary<Identifier, Type>(IdentifierComparer.Ordinal)
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
