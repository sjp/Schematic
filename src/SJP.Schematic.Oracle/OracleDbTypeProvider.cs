using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle
{
    public class OracleDbTypeProvider : IDbTypeProvider
    {
        public IDbType CreateColumnType(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            if (typeMetadata.TypeName == null)
                typeMetadata.TypeName = GetDefaultTypeName(typeMetadata);
            if (typeMetadata.DataType == DataType.Unknown)
            {
                typeMetadata.DataType = GetDataType(typeMetadata.TypeName);
                if (typeMetadata.DataType == DataType.Numeric)
                {
                    var numericPrecision = typeMetadata.NumericPrecision.Filter(np => np.Scale == 0);
                    numericPrecision.IfSome(np =>
                    {
                        typeMetadata.DataType = np.Precision < 8
                            ? DataType.Integer     // 2^32
                            : DataType.BigInteger; // note: could require storing in a decimal instead of long
                    });
                    if (typeMetadata.NumericPrecision.IsNone)
                    {
                        typeMetadata.DataType = typeMetadata.MaxLength < 8
                            ? DataType.Integer     // 2^32
                            : DataType.BigInteger; // note: could require storing in a decimal instead of long
                    }
                }
            }
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

            return FixedLengthTypes.Contains(typeName.LocalName);
        }

        protected static Identifier GetDefaultTypeName(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            switch (typeMetadata.DataType)
            {
                case DataType.BigInteger:
                    return new Identifier("SYS", "NUMBER");
                case DataType.Binary:
                case DataType.LargeBinary:
                    return typeMetadata.IsFixedLength
                        ? new Identifier("SYS", "RAW")
                        : new Identifier("SYS", "BLOB");
                case DataType.Boolean:
                    return new Identifier("SYS", "CHAR");
                case DataType.Date:
                    return new Identifier("SYS", "DATE");
                case DataType.DateTime:
                    return new Identifier("SYS", "TIMESTAMP WITH LOCAL TIME ZONE");
                case DataType.Float:
                    return new Identifier("SYS", "FLOAT");
                case DataType.Integer:
                    return new Identifier("SYS", "NUMBER");
                case DataType.Interval:
                    return new Identifier("sys", "TIMESTAMP WITH LOCAL TIME ZONE");
                case DataType.Numeric:
                case DataType.SmallInteger:
                    return new Identifier("SYS", "NUMBER");
                case DataType.String:
                case DataType.Text:
                    return typeMetadata.IsFixedLength
                        ? new Identifier("SYS", "CHAR")
                        : new Identifier("SYS", "VARCHAR2");
                case DataType.Time:
                    return new Identifier("SYS", "INTERVAL DAY TO SECOND");
                case DataType.Unicode:
                case DataType.UnicodeText:
                    return typeMetadata.IsFixedLength
                        ? new Identifier("SYS", "NCHAR")
                        : new Identifier("SYS", "NVARCHAR2");
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
            if (string.Equals(typeName.Schema, "SYS", StringComparison.OrdinalIgnoreCase))
                builder.Append(QuoteIdentifier(typeName.LocalName));
            else
                builder.Append(QuoteName(typeName));

            if (TypeNamesWithNoLengthAnnotation.Contains(typeName.LocalName))
                return builder.GetStringAndRelease();

            var npWithPrecisionOrScale = typeMetadata.NumericPrecision.Filter(np => np.Precision > 0 || np.Scale > 0);
            if (npWithPrecisionOrScale.IsSome)
            {
                npWithPrecisionOrScale.IfSome(precision =>
                {
                    builder.Append("(");
                    builder.Append(precision.Precision.ToString(CultureInfo.InvariantCulture));
                    if (precision.Scale > 0)
                    {
                        builder.Append(", ");
                        builder.Append(precision.Scale.ToString(CultureInfo.InvariantCulture));
                    }
                    builder.Append(")");
                });
            }
            else if (typeMetadata.MaxLength > 0)
            {
                builder.Append("(");
                var maxLength = typeMetadata.MaxLength;
                builder.Append(maxLength.ToString(CultureInfo.InvariantCulture));
                builder.Append(")");
            }

            return builder.GetStringAndRelease();
        }

        protected static DataType GetDataType(Identifier typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return StringToDataTypeMap.ContainsKey(typeName.LocalName)
                ? StringToDataTypeMap[typeName.LocalName]
                : DataType.Unknown;
        }

        protected static Type GetClrType(Identifier typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return StringToClrTypeMap.ContainsKey(typeName.LocalName)
                ? StringToClrTypeMap[typeName.LocalName]
                : typeof(object);
        }

        protected static string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return "\"" + identifier + "\"";
        }

        protected static string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var builder = StringBuilderCache.Acquire();

            if (name.Server != null)
                builder.Append(QuoteIdentifier(name.Server)).Append(".");
            if (name.Database != null)
                builder.Append(QuoteIdentifier(name.Database)).Append(".");
            if (name.Schema != null)
                builder.Append(QuoteIdentifier(name.Schema)).Append(".");
            if (name.LocalName != null)
                builder.Append(QuoteIdentifier(name.LocalName));

            return builder.GetStringAndRelease();
        }

        private static readonly IEnumerable<string> FixedLengthTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CHAR",
            "NCHAR",
            "RAW"
        };

        private static readonly IEnumerable<string> TypeNamesWithNoLengthAnnotation = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BFILE",
            "BINARY_FLOAT",
            "BINARY_DOUBLE",
            "BLOB",
            "CLOB",
            "DATE",
            "LONG",
            "LONG RAW",
            "NCLOB",
            "ROWID"
        };

        private static readonly IReadOnlyDictionary<string, DataType> StringToDataTypeMap = new Dictionary<string, DataType>(StringComparer.OrdinalIgnoreCase)
        {
            ["BFILE"] = DataType.LargeBinary,
            ["BINARY_DOUBLE"] = DataType.Float,
            ["BINARY_FLOAT"] = DataType.Float,
            ["BINARY_INTEGER"] = DataType.BigInteger,
            ["BLOB"] = DataType.LargeBinary,
            ["BOOLEAN"] = DataType.Boolean,
            ["CHAR"] = DataType.String,
            ["CLOB"] = DataType.String,
            ["DATE"] = DataType.Date,
            ["FLOAT"] = DataType.Float,
            ["INTEGER"] = DataType.BigInteger,
            ["INTERVAL YEAR TO MONTH"] = DataType.Integer,
            ["INTERVAL DAY TO SECOND"] = DataType.Time,
            ["LONG"] = DataType.String,
            ["LONG RAW"] = DataType.LargeBinary,
            ["NCHAR"] = DataType.Unicode,
            ["NCLOB"] = DataType.Unicode,
            ["NUMBER"] = DataType.Numeric,
            ["NVARCHAR2"] = DataType.Unicode,
            ["PLS_INTEGER"] = DataType.Integer,
            ["RAW"] = DataType.LargeBinary,
            ["REAL"] = DataType.Float,
            ["ROWID"] = DataType.String,
            ["TIMESTAMP"] = DataType.DateTime,
            ["TIMESTAMP WITH TIME ZONE"] = DataType.DateTime,
            ["TIMESTAMP WITH LOCAL TIME ZONE"] = DataType.DateTime,
            ["UROWID"] = DataType.String,
            ["UNSIGNED INTEGER"] = DataType.BigInteger,
            ["VARCHAR2"] = DataType.String,
            ["XMLTYPE"] = DataType.Unicode
        };

        private static readonly IReadOnlyDictionary<string, Type> StringToClrTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["BFILE"] = typeof(byte[]),
            ["BINARY_DOUBLE"] = typeof(double),
            ["BINARY_FLOAT"] = typeof(float),
            ["BINARY_INTEGER"] = typeof(long),
            ["BLOB"] = typeof(byte[]),
            ["BOOLEAN"] = typeof(bool),
            ["CHAR"] = typeof(string),
            ["CLOB"] = typeof(string),
            ["DATE"] = typeof(DateTime),
            ["FLOAT"] = typeof(decimal),
            ["INTEGER"] = typeof(decimal),
            ["INTERVAL YEAR TO MONTH"] = typeof(int),
            ["INTERVAL DAY TO SECOND"] = typeof(TimeSpan),
            ["LONG"] = typeof(string),
            ["LONG RAW"] = typeof(byte[]),
            ["NCHAR"] = typeof(string),
            ["NCLOB"] = typeof(string),
            ["NUMBER"] = typeof(decimal),
            ["NVARCHAR2"] = typeof(string),
            ["PLS_INTEGER"] = typeof(int),
            ["RAW"] = typeof(byte[]),
            ["REAL"] = typeof(decimal),
            ["ROWID"] = typeof(string),
            ["TIMESTAMP"] = typeof(DateTime),
            ["TIMESTAMP WITH TIME ZONE"] = typeof(DateTime),
            ["TIMESTAMP WITH LOCAL TIME ZONE"] = typeof(DateTime),
            ["UROWID"] = typeof(string),
            ["UNSIGNED INTEGER"] = typeof(decimal),
            ["VARCHAR2"] = typeof(string),
            ["XMLTYPE"] = typeof(string)
        };
    }
}
