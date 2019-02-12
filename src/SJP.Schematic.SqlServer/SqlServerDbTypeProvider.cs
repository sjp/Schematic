using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDbTypeProvider : IDbTypeProvider
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

        protected static bool GetIsFixedLength(Identifier typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return _fixedLengthTypes.Contains(typeName);
        }

        protected static Identifier GetDefaultTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            switch (typeMetadata.DataType)
            {
                case DataType.BigInteger:
                    return new Identifier("sys", "bigint");
                case DataType.Binary:
                case DataType.LargeBinary:
                    return typeMetadata.IsFixedLength
                        ? new Identifier("sys", "binary")
                        : new Identifier("sys", "varbinary");
                case DataType.Boolean:
                    return new Identifier("sys", "bit");
                case DataType.Date:
                case DataType.DateTime:
                    return new Identifier("sys", "datetime2");
                case DataType.Float:
                    return new Identifier("sys", "float");
                case DataType.Integer:
                    return new Identifier("sys", "int");
                case DataType.Interval:
                    return new Identifier("sys", "datetimeoffset");
                case DataType.Numeric:
                    return new Identifier("sys", "numeric");
                case DataType.SmallInteger:
                    return new Identifier("sys", "smallint");
                case DataType.String:
                case DataType.Text:
                    return typeMetadata.IsFixedLength
                        ? new Identifier("sys", "char")
                        : new Identifier("sys", "varchar");
                case DataType.Time:
                    return new Identifier("sys", "time");
                case DataType.Unicode:
                case DataType.UnicodeText:
                    return typeMetadata.IsFixedLength
                        ? new Identifier("sys", "nchar")
                        : new Identifier("sys", "nvarchar");
                case DataType.Unknown:
                    throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for an unknown data type.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for data type: " + typeMetadata.DataType.ToString(), nameof(typeMetadata));
            }
        }

        protected static string GetFormattedTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));
            if (typeMetadata.TypeName == null)
                throw new ArgumentException("The type name is missing. A formatted type name cannot be generated.", nameof(typeMetadata));

            var builder = StringBuilderCache.Acquire(typeMetadata.TypeName.LocalName.Length * 2);
            var typeName = typeMetadata.TypeName;
            if (string.Equals(typeName.Schema, "sys", StringComparison.OrdinalIgnoreCase))
                builder.Append(QuoteIdentifier(typeName.LocalName));
            else
                builder.Append(QuoteName(typeName));

            if (_typeNamesWithNoLengthAnnotation.Contains(typeName))
                return builder.GetStringAndRelease();

            builder.Append("(");

            if (typeMetadata.NumericPrecision.IsSome)
            {
                typeMetadata.NumericPrecision.Where(np => np.Precision > 0).IfSome(precision =>
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
            else
            {
                builder.Append("max");
            }

            builder.Append(")");

            return builder.GetStringAndRelease();
        }

        protected static DataType GetDataType(Identifier typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return _stringToDataTypeMap.ContainsKey(typeName)
                ? _stringToDataTypeMap[typeName]
                : DataType.Unknown;
        }

        protected static Type GetClrType(Identifier typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return _stringToClrTypeMap.ContainsKey(typeName)
                ? _stringToClrTypeMap[typeName]
                : typeof(object);
        }

        protected static string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return $"[{ identifier.Replace("]", "]]") }]";
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

        private static readonly IEnumerable<Identifier> _fixedLengthTypes = new HashSet<Identifier>(IdentifierComparer.OrdinalIgnoreCase)
        {
            new Identifier("sys", "char"),
            new Identifier("sys", "nchar"),
            new Identifier("sys", "binary")
        };

        private static readonly IEnumerable<Identifier> _typeNamesWithNoLengthAnnotation = new HashSet<Identifier>(IdentifierComparer.OrdinalIgnoreCase)
        {
            new Identifier("sys", "bigint"),
            new Identifier("sys", "bit"),
            new Identifier("sys", "date"),
            new Identifier("sys", "datetime"),
            new Identifier("sys", "image"),
            new Identifier("sys", "int"),
            new Identifier("sys", "money"),
            new Identifier("sys", "ntext"),
            new Identifier("sys", "rowversion"),
            new Identifier("sys", "smalldatetime"),
            new Identifier("sys", "smallint"),
            new Identifier("sys", "smallmoney"),
            new Identifier("sys", "sql_variant"),
            new Identifier("sys", "text"),
            new Identifier("sys", "timestamp"),
            new Identifier("sys", "tinyint"),
            new Identifier("sys", "uniqueidentifier"),
            new Identifier("sys", "xml")
        };

        private static readonly IReadOnlyDictionary<Identifier, DataType> _stringToDataTypeMap = new Dictionary<Identifier, DataType>(IdentifierComparer.OrdinalIgnoreCase)
        {
            [new Identifier("sys", "bigint")] = DataType.BigInteger,
            [new Identifier("sys", "binary")] = DataType.Binary,
            [new Identifier("sys", "bit")] = DataType.Boolean,
            [new Identifier("sys", "char")] = DataType.String,
            [new Identifier("sys", "date")] = DataType.Date,
            [new Identifier("sys", "datetime")] = DataType.DateTime,
            [new Identifier("sys", "datetime2")] = DataType.DateTime,
            [new Identifier("sys", "datetimeoffset")] = DataType.Time, // not sure on this, another type better?
            [new Identifier("sys", "decimal")] = DataType.Numeric,
            [new Identifier("sys", "float")] = DataType.Float,
            [new Identifier("sys", "image")] = DataType.Binary,
            [new Identifier("sys", "int")] = DataType.Integer,
            [new Identifier("sys", "money")] = DataType.Numeric,
            [new Identifier("sys", "nchar")] = DataType.Unicode,
            [new Identifier("sys", "ntext")] = DataType.UnicodeText,
            [new Identifier("sys", "numeric")] = DataType.Numeric,
            [new Identifier("sys", "nvarchar")] = DataType.Unicode,
            [new Identifier("sys", "real")] = DataType.Float,
            [new Identifier("sys", "rowversion")] = DataType.Binary,
            [new Identifier("sys", "smalldatetime")] = DataType.DateTime,
            [new Identifier("sys", "smallint")] = DataType.SmallInteger,
            [new Identifier("sys", "smallmoney")] = DataType.Numeric,
            [new Identifier("sys", "sql_variant")] = DataType.Unknown,
            [new Identifier("sys", "text")] = DataType.Text,
            [new Identifier("sys", "time")] = DataType.Time,
            [new Identifier("sys", "timestamp")] = DataType.Binary,
            [new Identifier("sys", "tinyint")] = DataType.SmallInteger,
            [new Identifier("sys", "uniqueidentifier")] = DataType.Unknown,
            [new Identifier("sys", "varbinary")] = DataType.Binary,
            [new Identifier("sys", "varchar")] = DataType.String,
            [new Identifier("sys", "xml")] = DataType.Unicode
        };

        private static readonly IReadOnlyDictionary<Identifier, Type> _stringToClrTypeMap = new Dictionary<Identifier, Type>(IdentifierComparer.OrdinalIgnoreCase)
        {
            [new Identifier("sys", "bigint")] = typeof(long),
            [new Identifier("sys", "binary")] = typeof(byte[]),
            [new Identifier("sys", "bit")] = typeof(bool),
            [new Identifier("sys", "char")] = typeof(string),
            [new Identifier("sys", "date")] = typeof(DateTime),
            [new Identifier("sys", "datetime")] = typeof(DateTime),
            [new Identifier("sys", "datetime2")] = typeof(DateTime),
            [new Identifier("sys", "datetimeoffset")] = typeof(DateTimeOffset),
            [new Identifier("sys", "decimal")] = typeof(decimal),
            [new Identifier("sys", "float")] = typeof(double),
            [new Identifier("sys", "image")] = typeof(byte[]),
            [new Identifier("sys", "int")] = typeof(int),
            [new Identifier("sys", "money")] = typeof(decimal),
            [new Identifier("sys", "nchar")] = typeof(string),
            [new Identifier("sys", "ntext")] = typeof(string),
            [new Identifier("sys", "numeric")] = typeof(decimal),
            [new Identifier("sys", "nvarchar")] = typeof(string),
            [new Identifier("sys", "real")] = typeof(float),
            [new Identifier("sys", "rowversion")] = typeof(byte[]),
            [new Identifier("sys", "smalldatetime")] = typeof(DateTime),
            [new Identifier("sys", "smallint")] = typeof(short),
            [new Identifier("sys", "smallmoney")] = typeof(decimal),
            [new Identifier("sys", "sql_variant")] = typeof(object),
            [new Identifier("sys", "text")] = typeof(string),
            [new Identifier("sys", "time")] = typeof(TimeSpan),
            [new Identifier("sys", "timestamp")] = typeof(byte[]),
            [new Identifier("sys", "tinyint")] = typeof(byte),
            [new Identifier("sys", "uniqueidentifier")] = typeof(Guid),
            [new Identifier("sys", "varbinary")] = typeof(byte[]),
            [new Identifier("sys", "varchar")] = typeof(string),
            [new Identifier("sys", "xml")] = typeof(string)
        };
    }
}
