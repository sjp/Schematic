using System;
using System.Reflection;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTableComputedColumn : IDatabaseComputedColumn
    {
        public ReflectionTableComputedColumn(IDatabaseDialect dialect, IRelationalDatabaseTable table, PropertyInfo prop, string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
            Property = prop ?? throw new ArgumentNullException(nameof(prop));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Name = dialect.GetAliasOrDefault(prop);
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Type = new ReflectionComputedColumnDataType();
            IsNullable = true;
        }

        public bool IsNullable { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        public IDbType Type { get; }

        public string DefaultValue { get; }

        public bool IsAutoIncrement { get; }

        public bool IsComputed { get; } = true;

        public string Definition { get; }

        protected IDatabaseDialect Dialect { get; }

        protected PropertyInfo Property { get; }

        protected class ReflectionComputedColumnDataType : IDbType
        {
            public DataType Type { get; } = DataType.Unknown;

            public bool IsFixedLength { get; }

            public int Length => -1;

            public Type ClrType => typeof(object);
        }
    }
}
