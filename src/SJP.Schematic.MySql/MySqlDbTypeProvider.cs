using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql
{
    /// <summary>
    /// A database type provider for MySQL.
    /// </summary>
    /// <seealso cref="IDbTypeProvider" />
    public class MySqlDbTypeProvider : IDbTypeProvider
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
                typeMetadata.DataType = GetDataType(typeMetadata.TypeName.LocalName);
            if (typeMetadata.ClrType == null)
                typeMetadata.ClrType = GetClrType(typeMetadata.TypeName.LocalName);
            typeMetadata.IsFixedLength = GetIsFixedLength(typeMetadata.TypeName.LocalName);

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
        protected static bool GetIsFixedLength(string typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
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

            switch (typeMetadata.DataType)
            {
                case DataType.BigInteger:
                    return "bigint";
                case DataType.Binary:
                    return typeMetadata.IsFixedLength ? "binary" : "varbinary";
                case DataType.LargeBinary:
                    return "longblob";
                case DataType.Boolean:
                    return "bit";
                case DataType.Date:
                    return "date";
                case DataType.DateTime:
                    return "datetime";
                case DataType.Float:
                    return "double";
                case DataType.Integer:
                    return "int";
                case DataType.Interval:
                    return "timestamp";
                case DataType.Numeric:
                    return "numeric";
                case DataType.SmallInteger:
                    return "smallint";
                case DataType.String:
                case DataType.Unicode:
                    return typeMetadata.IsFixedLength ? "char" : "varchar";
                case DataType.Text:
                case DataType.UnicodeText:
                    return "longtext";
                case DataType.Time:
                    return "time";
                case DataType.Unknown:
                    throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for an unknown data type.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for data type: " + typeMetadata.DataType.ToString());
            }
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
            var typeName = typeMetadata.TypeName.LocalName;

            builder.Append(typeName);

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
                var maxLength = typeMetadata.DataType == DataType.Unicode || typeMetadata.DataType == DataType.UnicodeText;
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
        protected static DataType GetDataType(string typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
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
        protected static Type GetClrType(string typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(typeName));

            return StringToClrTypeMap.ContainsKey(typeName)
                ? StringToClrTypeMap[typeName]
                : typeof(object);
        }

        private static readonly IEnumerable<string> FixedLengthTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "char",
            "binary"
        };

        private static readonly IEnumerable<string> TypeNamesWithNoLengthAnnotation = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bit",
            "tinyint",
            "smallint",
            "mediumint",
            "int",
            "bigint",
            "double",
            "tinyblob",
            "blob",
            "mediumblob",
            "largeblob",
            "tinytext",
            "text",
            "mediumtext",
            "longtext"
        };

        private static readonly IReadOnlyDictionary<string, DataType> StringToDataTypeMap = new Dictionary<string, DataType>(StringComparer.OrdinalIgnoreCase)
        {
            ["bit"] = DataType.Boolean,
            ["tinyint"] = DataType.Integer,
            ["smallint"] = DataType.Integer,
            ["mediumint"] = DataType.Integer,
            ["int"] = DataType.Integer,
            ["bigint"] = DataType.Integer,
            ["numeric"] = DataType.Numeric,
            ["decimal"] = DataType.Numeric,
            ["float"] = DataType.Float,
            ["double"] = DataType.Float,
            ["date"] = DataType.Date,
            ["datetime"] = DataType.DateTime,
            ["timestamp"] = DataType.DateTime,
            ["time"] = DataType.Time,
            ["year"] = DataType.Integer,
            ["char"] = DataType.Unicode,
            ["varchar"] = DataType.Unicode,
            ["binary"] = DataType.Binary,
            ["varbinary"] = DataType.Binary,
            ["tinyblob"] = DataType.LargeBinary,
            ["blob"] = DataType.LargeBinary,
            ["mediumblob"] = DataType.LargeBinary,
            ["largeblob"] = DataType.LargeBinary,
            ["tinytext"] = DataType.UnicodeText,
            ["text"] = DataType.UnicodeText,
            ["mediumtext"] = DataType.UnicodeText,
            ["longtext"] = DataType.UnicodeText
        };

        private static readonly IReadOnlyDictionary<string, Type> StringToClrTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["bit"] = typeof(bool),
            ["tinyint"] = typeof(byte),
            ["smallint"] = typeof(short),
            ["mediumint"] = typeof(int),
            ["int"] = typeof(int),
            ["bigint"] = typeof(long),
            ["decimal"] = typeof(decimal),
            ["numeric"] = typeof(decimal),
            ["float"] = typeof(double),
            ["double"] = typeof(double),
            ["date"] = typeof(DateTime),
            ["datetime"] = typeof(DateTime),
            ["timestamp"] = typeof(DateTime),
            ["time"] = typeof(DateTime),
            ["year"] = typeof(int),
            ["char"] = typeof(string),
            ["varchar"] = typeof(string),
            ["tinytext"] = typeof(string),
            ["text"] = typeof(string),
            ["mediumtext"] = typeof(string),
            ["longtext"] = typeof(string),
            ["binary"] = typeof(byte[]),
            ["varbinary"] = typeof(byte[]),
            ["tinyblob"] = typeof(byte[]),
            ["blob"] = typeof(byte[]),
            ["mediumblob"] = typeof(byte[]),
            ["largeblob"] = typeof(byte[])
        };
    }
}
