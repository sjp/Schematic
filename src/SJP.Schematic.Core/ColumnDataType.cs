using System;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class ColumnDataType : IDbType
    {
        public ColumnDataType(
            Identifier typeName,
            DataType dataType,
            string definition,
            Type clrType,
            bool isFixedLength,
            int maxLength,
            Option<NumericPrecision> numericPrecision,
            Option<Identifier> collation
        )
        {
            if (!dataType.IsValid())
                throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            DataType = dataType;
            Definition = definition;
            ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
            IsFixedLength = isFixedLength;
            MaxLength = maxLength;
            NumericPrecision = numericPrecision;
            Collation = collation;
        }

        public Identifier TypeName { get; }

        public DataType DataType { get; }

        public string Definition { get; }

        public bool IsFixedLength { get; }

        public int MaxLength { get; }

        public Type ClrType { get; }

        public Option<NumericPrecision> NumericPrecision { get; }

        public Option<Identifier> Collation { get; }
    }
}
