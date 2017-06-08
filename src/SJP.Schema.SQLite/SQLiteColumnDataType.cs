using System;
using System.Collections.Generic;
using System.Data;
using SJP.Schema.Core;

namespace SJP.Schema.SQLite
{
    public class SQLiteColumnDataType : IDbType
    {
        public SQLiteColumnDataType(Identifier typeName)
            : this(typeName, UnknownLength)
        {
        }

        public SQLiteColumnDataType(Identifier typeName, int length)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            PhysicalTypeName = typeName.LocalName.ToLowerInvariant();
            Type = GetDataTypeFromName(PhysicalTypeName);
            ClrType = GetClrTypeFromTypeName(PhysicalTypeName);
            Length = length;

            IsNumericType = _numericTypes.Contains(Type);
            IsStringType = _stringTypes.Contains(Type);
        }

        public DataType Type { get; }

        public string PhysicalTypeName { get; }

        public bool IsFixedLength { get; }

        public int Length { get; }

        public Type ClrType { get; }

        public bool IsNumericType { get; }

        public bool IsStringType { get; }

        public const int UnknownLength = -1;

        private static DataType GetDataTypeFromName(string typeName)
        {
            if (_stringToDataTypeMap.ContainsKey(typeName))
                return _stringToDataTypeMap[typeName];

            return DataType.Binary;
        }

        private static Type GetClrTypeFromTypeName(string typeName)
        {
            if (_stringToClrTypeMap.ContainsKey(typeName))
                return _stringToClrTypeMap[typeName];

            return typeof(object);
        }

        private readonly static IReadOnlyDictionary<string, DataType> _stringToDataTypeMap = new Dictionary<string, DataType>(StringComparer.OrdinalIgnoreCase)
        {
            ["INT"] = DataType.Integer,
            ["INTEGER"] = DataType.Integer,
            ["TINYINT"] = DataType.Integer,
            ["SMALLINT"] = DataType.Integer,
            ["MEDIUMINT"] = DataType.Integer,
            ["BIGINT"] = DataType.Integer,
            ["UNSIGNED BIG INT"] = DataType.Integer,
            ["INT2"] = DataType.Integer,
            ["INT8"] = DataType.Integer,

            ["NUMERIC"] = DataType.Numeric,
            ["DECIMAL"] = DataType.Numeric,
            ["BOOLEAN"] = DataType.Boolean,
            ["DATE"] = DataType.Integer,
            ["DATETIME"] = DataType.Integer,

            ["DOUBLE"] = DataType.Float,
            ["DOUBLE PRECISION"] = DataType.Float,
            ["FLOAT"] = DataType.Float,
            ["REAL"] = DataType.Float,

            ["CHARACTER"] = DataType.Unicode,
            ["VARCHAR"] = DataType.Unicode,
            ["VARYING CHARACTER"] = DataType.Unicode,
            ["NCHAR"] = DataType.Unicode,
            ["NATIVE CHARACTER"] = DataType.Unicode,
            ["NVARCHAR"] = DataType.Unicode,
            ["TEXT"] = DataType.Unicode,
            ["CLOB"] = DataType.Unicode
        };

        private readonly static IReadOnlyDictionary<string, Type> _stringToClrTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["INT"] = typeof(int),
            ["INTEGER"] = typeof(int),
            ["TINYINT"] = typeof(int),
            ["SMALLINT"] = typeof(int),
            ["MEDIUMINT"] = typeof(int),
            ["BIGINT"] = typeof(int),
            ["UNSIGNED BIG INT"] = typeof(int),
            ["INT2"] = typeof(int),
            ["INT8"] = typeof(int),

            ["NUMERIC"] = typeof(decimal),
            ["DECIMAL"] = typeof(decimal),
            ["BOOLEAN"] = typeof(bool),
            ["DATE"] = typeof(int),
            ["DATETIME"] = typeof(int),

            ["DOUBLE"] = typeof(double),
            ["DOUBLE PRECISION"] = typeof(double),
            ["FLOAT"] = typeof(double),
            ["REAL"] = typeof(double),

            ["CHARACTER"] = typeof(string),
            ["VARCHAR"] = typeof(string),
            ["VARYING CHARACTER"] = typeof(string),
            ["NCHAR"] = typeof(string),
            ["NATIVE CHARACTER"] = typeof(string),
            ["NVARCHAR"] = typeof(string),
            ["TEXT"] = typeof(string),
            ["CLOB"] = typeof(string)
        };

        private readonly static ISet<DataType> _numericTypes = new HashSet<DataType> { DataType.BigInteger, DataType.Float, DataType.Integer, DataType.Numeric, DataType.SmallInteger };

        private readonly static ISet<DataType> _stringTypes = new HashSet<DataType> { DataType.String, DataType.Text, DataType.Unicode, DataType.UnicodeText };
    }

    public class SQLiteNumericColumnDataType : SQLiteColumnDataType, IDbNumericType
    {
        public SQLiteNumericColumnDataType(Identifier typeName)
            : base(typeName, UnknownLength)
        {
            Precision = UnknownLength;
            Scale = UnknownLength;
        }

        public SQLiteNumericColumnDataType(Identifier typeName, int precision, int scale = 0)
            : base(typeName, precision)
        {
            Precision = precision;
            Scale = scale;
        }

        public int Precision { get; }

        public int Scale { get; }
    }

    public class SQLiteStringColumnDataType : SQLiteColumnDataType, IDbStringType
    {
        public SQLiteStringColumnDataType(Identifier typeName, int length, string collationName = null)
            : base(typeName, length)
        {
            IsUnicode = _unicodeTypes.Contains(Type);
            Collation = collationName;
        }

        public bool IsUnicode { get; }

        public string Collation { get; }

        private readonly static ISet<DataType> _unicodeTypes = new HashSet<DataType> { DataType.Unicode, DataType.UnicodeText };
    }
}
