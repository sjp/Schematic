using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTableComputedColumn : IDatabaseComputedColumn
    {
        public ReflectionTableComputedColumn(IDatabaseDialect dialect, IRelationalDatabaseTable table, Identifier columnName, string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Name = columnName;
            Table = table ?? throw new ArgumentNullException(nameof(table));
            IsNullable = true;
        }

        public bool IsNullable { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        public IDbType Type => _unknownType;

        public string DefaultValue { get; }

        public Option<IAutoIncrement> AutoIncrement { get; }

        public bool IsComputed { get; } = true;

        public string Definition { get; }

        protected IDatabaseDialect Dialect { get; }

        protected readonly IDbType _unknownType = new ReflectionComputedColumnDataType();

        // represents an unknown datatype
        protected sealed class ReflectionComputedColumnDataType : IDbType
        {
            public Identifier TypeName { get; }

            public DataType DataType { get; }

            public string Definition { get; }

            public Option<NumericPrecision> NumericPrecision { get; } = Option<NumericPrecision>.None;

            public Option<Identifier> Collation { get; } = Option<Identifier>.None;

            public bool IsFixedLength { get; }

            public int MaxLength { get; }

            public Type ClrType { get; } = typeof(object);
        }
    }
}
