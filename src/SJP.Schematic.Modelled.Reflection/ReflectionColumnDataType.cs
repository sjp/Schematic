using System;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionColumnDataType : IDbType
    {
        public ReflectionColumnDataType(IDatabaseDialect dialect, Type columnType, Type clrType)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (columnType == null)
                throw new ArgumentNullException(nameof(columnType));
            if (clrType == null)
                throw new ArgumentNullException(nameof(clrType));

            var attr = dialect.GetDialectAttribute<DeclaredTypeAttribute>(columnType);
            if (attr == null)
                throw new ArgumentException($"The column type { columnType.FullName } does not contain a definition for the dialect { dialect.GetType().FullName }");

            var collationAttr = dialect.GetDialectAttribute<CollationAttribute>(columnType);
            var typeMetadata = new ColumnTypeMetadata
            {
                ClrType = clrType,
                Collation = collationAttr?.CollationName,
                DataType = attr.DataType,
                IsFixedLength = attr.IsFixedLength,
                MaxLength = attr.Length,
                NumericPrecision = new NumericPrecision(attr.Precision, attr.Scale),
            };
            var dbType = dialect.CreateColumnType(typeMetadata);

            // map dbType to properties, avoids keeping a reference
            TypeName = dbType.TypeName;
            DataType = dbType.DataType;
            Definition = dbType.Definition;
            IsFixedLength = dbType.IsFixedLength;
            MaxLength = dbType.MaxLength;
            ClrType = dbType.ClrType;
            NumericPrecision = dbType.NumericPrecision;
            Collation = dbType.Collation;
        }

        public Identifier TypeName { get; }

        public DataType DataType { get; }

        public string Definition { get; }

        public bool IsFixedLength { get; }

        public int MaxLength { get; }

        public Type ClrType { get; }

        public NumericPrecision NumericPrecision { get; }

        public Identifier Collation { get; }
    }
}
