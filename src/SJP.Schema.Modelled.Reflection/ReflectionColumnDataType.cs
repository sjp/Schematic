using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionColumnDataType : IDbType
    {
        public ReflectionColumnDataType(Type columnType, Type clrType, IDatabaseDialect dialect)
        {
            if (columnType == null)
                throw new ArgumentNullException(nameof(columnType));
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));

            var attr = dialect.GetDialectAttribute<DeclaredTypeAttribute>(columnType);
            if (attr == null)
                throw new ArgumentException($"The column type { columnType.FullName } does not contain a definition for the dialect { dialect.GetType().FullName }");

            Type = attr.DataType;
            IsFixedLength = attr.IsFixedLength;
            Length = attr.Length;
            ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));

            IsNumericType = _numericTypes.Contains(Type);
            IsStringType = _stringTypes.Contains(Type);
            _typeAttribute = attr;
            _columnType = columnType;
            _dialect = dialect;
        }

        public DataType Type { get; }

        public bool IsFixedLength { get; }

        public int Length { get; }

        public Type ClrType { get; }

        public bool IsNumericType { get; }

        public bool IsStringType { get; }

        public IDbNumericType AsNumericType()
        {
            if (!IsNumericType)
                throw new InvalidOperationException($"This column type is not a numeric type. It is a '{ Type }' type.");

            return new ReflectionNumericColumnDataType(_typeAttribute, ClrType);
        }

        public IDbStringType AsStringType()
        {
            if (!IsStringType)
                throw new InvalidOperationException($"This column type is not a string type. It is a '{ Type }' type.");

            var collationAttr = _dialect.GetDialectAttribute<CollationAttribute>(_columnType);
            return new ReflectionStringColumnDataType(_typeAttribute, ClrType, collationAttr?.CollationName);
        }

        private readonly Type _columnType; // in case child classes need other attributes
        private readonly IDatabaseDialect _dialect;
        private readonly DeclaredTypeAttribute _typeAttribute;

        private readonly static ISet<DataType> _numericTypes = new HashSet<DataType> { DataType.BigInteger, DataType.Float, DataType.Integer, DataType.Numeric, DataType.SmallInteger };

        private readonly static ISet<DataType> _stringTypes = new HashSet<DataType> { DataType.String, DataType.Text, DataType.Unicode, DataType.UnicodeText };

        private class ReflectionNumericColumnDataType : IDbNumericType
        {
            public ReflectionNumericColumnDataType(DeclaredTypeAttribute typeAttr, Type clrType)
            {
                ClrType = clrType;
                Precision = typeAttr.Precision;
                Scale = typeAttr.Precision;
                Type = typeAttr.DataType;
                IsFixedLength = typeAttr.IsFixedLength;
                Length = typeAttr.Length;
            }

            public int Precision { get; }

            public int Scale { get; }

            public DataType Type { get; }

            public bool IsFixedLength { get; }

            public int Length { get; }

            public Type ClrType { get; }
        }

        private class ReflectionStringColumnDataType : IDbStringType
        {
            public ReflectionStringColumnDataType(DeclaredTypeAttribute typeAttr, Type clrType, string collationName)
            {
                Type = typeAttr.DataType;
                ClrType = clrType;
                IsUnicode = _unicodeTypes.Contains(Type);
                Collation = collationName;
                IsFixedLength = typeAttr.IsFixedLength;
                Length = typeAttr.Length;
            }

            public DataType Type { get; }

            public bool IsFixedLength { get; }

            public int Length { get; }

            public Type ClrType { get; }

            public bool IsUnicode { get; }

            public string Collation { get; }

            private readonly static ISet<DataType> _unicodeTypes = new HashSet<DataType> { DataType.Unicode, DataType.UnicodeText };
        }
    }
}
