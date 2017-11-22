using System;
using System.Reflection;
using SJP.Schematic.Core;

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
            Type = new ReflectionComputedColumnDataType();
            IsNullable = true;
        }

        public bool IsNullable { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        public IDbType Type { get; }

        public string DefaultValue { get; }

        public IAutoIncrement AutoIncrement { get; }

        public bool IsComputed { get; } = true;

        public string Definition { get; }

        protected IDatabaseDialect Dialect { get; }

        protected class ReflectionComputedColumnDataType : IDbType
        {
            public DataType Type { get; }

            public bool IsFixedLength { get; }

            public int Length => -1;

            public Type ClrType => typeof(object);
        }
    }
}
