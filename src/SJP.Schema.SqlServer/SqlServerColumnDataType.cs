using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerColumnDataType : IDbType
    {
        public SqlServerColumnDataType(Identifier typeName)
            : this(typeName, UnknownLength)
        {
        }

        public SqlServerColumnDataType(Identifier typeName, int length)
        {
            if (typeName == null || typeName.LocalName == null)
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

        protected static int UnknownLength = -1;

        private static DataType GetDataTypeFromName(string typeName)
        {
            if (_stringToDataTypeMap.ContainsKey(typeName))
                return _stringToDataTypeMap[typeName];

            return DataType.Unknown;
        }

        private static Type GetClrTypeFromTypeName(string typeName)
        {
            if (_stringToClrTypeMap.ContainsKey(typeName))
                return _stringToClrTypeMap[typeName];

            return typeof(object);
        }

        private readonly static IReadOnlyDictionary<string, DataType> _stringToDataTypeMap = new Dictionary<string, DataType>(StringComparer.OrdinalIgnoreCase)
        {
            ["bigint"] = DataType.BigInteger,
            ["binary"] = DataType.Binary,
            ["bit"] = DataType.Boolean,
            ["char"] = DataType.String,
            ["date"] = DataType.Date,
            ["datetime"] = DataType.DateTime,
            ["datetime2"] = DataType.DateTime,
            ["datetimeoffset"] = DataType.Time, // not sure on this, another type better?
            ["decimal"] = DataType.Numeric,
            ["float"] = DataType.Float,
            ["image"] = DataType.Binary,
            ["int"] = DataType.Integer,
            ["money"] = DataType.Numeric,
            ["nchar"] = DataType.Unicode,
            ["ntext"] = DataType.UnicodeText,
            ["numeric"] = DataType.Numeric,
            ["nvarchar"] = DataType.Unicode,
            ["real"] = DataType.Float,
            ["rowversion"] = DataType.Binary,
            ["smalldatetime"] = DataType.DateTime,
            ["smallint"] = DataType.SmallInteger,
            ["smallmoney"] = DataType.Numeric,
            ["sql_variant"] = DataType.Unknown,
            ["text"] = DataType.Text,
            ["time"] = DataType.Time,
            ["timestamp"] = DataType.Binary,
            ["tinyint"] = DataType.SmallInteger,
            ["uniqueidentifier"] = DataType.Unknown,
            ["varbinary"] = DataType.Binary,
            ["varchar"] = DataType.String,
            ["xml"] = DataType.Unicode
        };

        private readonly static IReadOnlyDictionary<string, Type> _stringToClrTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["bigint"] = typeof(long),
            ["binary"] = typeof(byte[]),
            ["bit"] = typeof(bool),
            ["char"] = typeof(string),
            ["date"] = typeof(DateTime),
            ["datetime"] = typeof(DateTime),
            ["datetime2"] = typeof(DateTime),
            ["datetimeoffset"] = typeof(DateTimeOffset),
            ["decimal"] = typeof(decimal),
            ["float"] = typeof(double),
            ["image"] = typeof(byte[]),
            ["int"] = typeof(int),
            ["money"] = typeof(decimal),
            ["nchar"] = typeof(string),
            ["ntext"] = typeof(string),
            ["numeric"] = typeof(decimal),
            ["nvarchar"] = typeof(string),
            ["real"] = typeof(float),
            ["rowversion"] = typeof(byte[]),
            ["smalldatetime"] = typeof(DateTime),
            ["smallint"] = typeof(short),
            ["smallmoney"] = typeof(decimal),
            ["sql_variant"] = typeof(object),
            ["text"] = typeof(string),
            ["time"] = typeof(TimeSpan),
            ["timestamp"] = typeof(byte[]),
            ["tinyint"] = typeof(byte),
            ["uniqueidentifier"] = typeof(Guid),
            ["varbinary"] = typeof(byte[]),
            ["varchar"] = typeof(string),
            ["xml"] = typeof(string)
        };

        private readonly static ISet<DataType> _numericTypes = new HashSet<DataType> { DataType.BigInteger, DataType.Float, DataType.Integer, DataType.Numeric, DataType.SmallInteger };

        private readonly static ISet<DataType> _stringTypes = new HashSet<DataType> { DataType.String, DataType.Text, DataType.Unicode, DataType.UnicodeText };
    }

    public class SqlServerNumericColumnDataType : SqlServerColumnDataType, IDbNumericType
    {
        public SqlServerNumericColumnDataType(Identifier typeName)
            : base(typeName, UnknownLength)
        {
            Precision = UnknownLength;
            Scale = UnknownLength;
        }

        public SqlServerNumericColumnDataType(Identifier typeName, int precision, int scale = 0)
            : base(typeName, precision)
        {
            Precision = precision;
            Scale = scale;
        }

        public int Precision { get; }

        public int Scale { get; }
    }

    public class SqlServerStringColumnDataType : SqlServerColumnDataType, IDbStringType
    {
        public SqlServerStringColumnDataType(Identifier typeName, int length, string collationName = null)
            : base(typeName, length)
        {
            IsUnicode = _unicodeTypes.Contains(Type);
            Collation = collationName;
        }

        public bool IsUnicode { get; }

        public string Collation { get; }

        private readonly static ISet<DataType> _unicodeTypes = new HashSet<DataType> { DataType.Unicode, DataType.UnicodeText };

        // set of column types whose length is stored as 16, when it should be -1
        private static readonly ISet<string> _unboundedSystemTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "text", "ntext", "image" };
    }
}
